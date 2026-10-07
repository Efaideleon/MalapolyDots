using System.Collections.Generic;
using Assets.Common;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.DataComponents;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.Authorings;
using DOTS.DataComponents;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    public struct AiOpponent : IComponentData
    {
        public double NextDecision;
        public int ObservedState, BuildingsThisTurn;
    }

    // Server-created requests use the same rule handlers as authenticated player RPCs.
    public struct AiActionRequest : IComponentData { public Entity Player; }
    public struct AiTradeOffer : IComponentData { public Entity Buyer; public int OfferId, PropertyId, Price; }

    public static class GameplayActionSource
    {
        public static int PlayerId(EntityManager em, Entity request, Entity connection)
        {
            if (em.HasComponent<AiActionRequest>(request))
            {
                var player = em.GetComponentData<AiActionRequest>(request).Player;
                if (em.HasComponent<AiOpponent>(player) && em.HasComponent<GhostOwner>(player) &&
                    em.HasComponent<BankruptPlayer>(player) && !em.GetComponentData<BankruptPlayer>(player).Value &&
                    !em.GetComponentData<BankruptPlayer>(player).Declared)
                    return em.GetComponentData<GhostOwner>(player).NetworkId;
                return 0;
            }
            return em.HasComponent<NetworkId>(connection) ? em.GetComponentData<NetworkId>(connection).Value : 0;
        }

        public static bool Owns(EntityManager em, Entity request, Entity connection, Entity player) =>
            em.HasComponent<GhostOwner>(player) && PlayerId(em, request, connection) > 0 &&
            PlayerId(em, request, connection) == em.GetComponentData<GhostOwner>(player).NetworkId;

        public static Entity Queue<T>(EntityManager em, Entity player, T action) where T : unmanaged, IRpcCommand
        {
            var request = em.CreateEntity(typeof(T), typeof(ReceiveRpcCommandRequest), typeof(AiActionRequest));
            if (!ComponentType.ReadWrite<T>().IsZeroSized) em.SetComponentData(request, action);
            em.SetComponentData(request, new AiActionRequest { Player = player });
            return request;
        }
    }

    public static class SoloOpponentSetup
    {
        public static bool Prepare(EntityManager em, int humanId, CharactersEnum humanCharacter)
        {
            using var query = em.CreateEntityQuery(typeof(PrepickedCharacter));
            using var entities = query.ToEntityArray(Allocator.Temp);
            var choices = new List<Entity>();
            Entity human = Entity.Null;
            foreach (var entity in entities)
            {
                var choice = em.GetComponentData<PrepickedCharacter>(entity);
                if (choice.Character == humanCharacter) human = entity;
                else if (choice.Character != CharactersEnum.Default && choice.Prefab != Entity.Null) choices.Add(entity);
            }
            int count = System.Math.Clamp(SoloSession.Opponents, 1, 5);
            if (human == Entity.Null || choices.Count < count) return false;
            choices.Sort((a, b) => em.GetComponentData<PrepickedCharacter>(a).Character.CompareTo(em.GetComponentData<PrepickedCharacter>(b).Character));
            foreach (var entity in entities)
            {
                var choice = em.GetComponentData<PrepickedCharacter>(entity);
                choice.PrePicked = entity == human;
                choice.OwnerNetworkId = entity == human ? humanId : 0;
                em.SetComponentData(entity, choice);
            }
            for (int i = 0; i < count; i++)
            {
                var choice = em.GetComponentData<PrepickedCharacter>(choices[i]);
                choice.PrePicked = true;
                choice.OwnerNetworkId = SoloSession.FirstAiId + i;
                em.SetComponentData(choices[i], choice);
            }
            return true;
        }
    }

    public static class AiStrategy
    {
        public static bool BuyDeed(int cash, int price, bool completesSet = false) =>
            price > 0 && (long)cash - price >= (completesSet ? 75 : 150);

        public static bool AcceptOffer(int cash, int price, int value, bool completesSet) =>
            price > 0 && value > 0 && (long)cash - price >= 100 &&
            price <= (long)value * (completesSet ? 125 : 100) / 100;

        public static bool CompletesSet(EntityManager em, Entity deed, Entity player, NativeArray<Entity> deeds)
        {
            if (!em.HasComponent<ColorCodeComponent>(deed)) return false;
            var color = em.GetComponentData<ColorCodeComponent>(deed).Value;
            if (color < PropertyColor.Brown) return false;
            int members = 0;
            foreach (var other in deeds)
            {
                if (!em.HasComponent<ColorCodeComponent>(other) || em.GetComponentData<ColorCodeComponent>(other).Value != color) continue;
                members++;
                if (other != deed && (!em.HasComponent<OwnerByEntityComponent>(other) ||
                    em.GetComponentData<OwnerByEntityComponent>(other).Entity != player)) return false;
            }
            return members >= 2;
        }
    }
}

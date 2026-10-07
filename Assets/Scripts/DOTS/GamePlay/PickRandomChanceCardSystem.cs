using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public struct ChanceDeckEntry : IBufferElementData
    {
        public int CardId;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(GamePlaySystem))]
    public partial struct PickRandomChanceCardSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<ChanceCardPicked>();
            state.EntityManager.CreateSingletonBuffer<ChanceDeckEntry>();
            state.RequireForUpdate<RandomValueComponent>();
            state.RequireForUpdate<GhostChanceCardPicked>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<SpaceLandedOn>();
            state.RequireForUpdate<ChanceActionDataBuffer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.GetSingleton<GameStateComponent>().State != GameState.Landing) return;
            using var feedback = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (space, picked, resolved, player) in SystemAPI.Query<RefRO<SpaceLandedOn>, RefRW<GhostChanceCardPicked>, RefRW<LandingPaymentResolved>>()
                         .WithAll<ActivePlayer>().WithEntityAccess())
            {
                if (resolved.ValueRO.Value || !SystemAPI.HasComponent<ChanceSpaceTag>(space.ValueRO.entity)) continue;
                var cards = SystemAPI.GetBuffer<ChanceActionDataBuffer>(space.ValueRO.entity);
                if (cards.Length == 0) continue;
                var deck = SystemAPI.GetSingletonBuffer<ChanceDeckEntry>();
                if (deck.Length == 0)
                {
                    foreach (var card in cards) deck.Add(new ChanceDeckEntry { CardId = card.id });
                    var random = SystemAPI.GetSingletonRW<RandomValueComponent>();
                    for (int i = deck.Length - 1; i > 0; i--)
                    {
                        int j = random.ValueRW.Value.NextInt(0, i + 1);
                        var previous = deck[i]; deck[i] = deck[j]; deck[j] = previous;
                    }
                }
                int id = deck[deck.Length - 1].CardId;
                deck.RemoveAt(deck.Length - 1);
                foreach (var card in cards)
                {
                    if (card.id != id) continue;
                    resolved.ValueRW.Value = true;
                    picked.ValueRW.DrawSequence++;
                    if (picked.ValueRO.DrawSequence == 0) picked.ValueRW.DrawSequence = 1;
                    picked.ValueRW.id = card.id;
                    picked.ValueRW.msg = card.msg;
                    picked.ValueRW.effect = card.effect;
                    picked.ValueRW.amount = ChanceCardRules.Apply(state.EntityManager, feedback, player, card);
                    break;
                }
            }
            feedback.Playback(state.EntityManager);
        }
    }
}

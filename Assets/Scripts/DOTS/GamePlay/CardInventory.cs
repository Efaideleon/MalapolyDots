using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.Mediator;
using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    public enum PlayerCardType : byte
    {
        None = 0,
        GetOutOfJailFree = 1,
        Monopoly = 2
    }

    public struct UsePlayerCardRpc : IRpcCommand
    {
        public byte Slot;
        public int Color;
    }

    public struct ResolveCardOverflowRpc : IRpcCommand
    {
        // 255 discards the newly drawn card; 0-3 discard an existing hand slot.
        public byte DiscardSlot;
    }

    public static class CardInventoryRules
    {
        public const int Capacity = 4;
        public const byte DiscardNewCard = byte.MaxValue;

        public static PlayerCardType GetCard(PlayerCardInventory inventory, int slot) => slot switch
        {
            0 => (PlayerCardType)inventory.Card0,
            1 => (PlayerCardType)inventory.Card1,
            2 => (PlayerCardType)inventory.Card2,
            3 => (PlayerCardType)inventory.Card3,
            _ => PlayerCardType.None
        };

        static void SetCard(ref PlayerCardInventory inventory, int slot, PlayerCardType card)
        {
            byte value = (byte)card;
            switch (slot)
            {
                case 0: inventory.Card0 = value; break;
                case 1: inventory.Card1 = value; break;
                case 2: inventory.Card2 = value; break;
                case 3: inventory.Card3 = value; break;
            }
        }

        public static bool Receive(EntityManager manager, Entity player, PlayerCardType card)
        {
            if (card == PlayerCardType.None) return false;
            if (!manager.HasComponent<PlayerCardInventory>(player))
            {
                // Preserve legacy behavior for editor/test entities that predate the hand.
                if (card == PlayerCardType.GetOutOfJailFree && manager.HasComponent<JailState>(player))
                {
                    var jail = manager.GetComponentData<JailState>(player);
                    jail.HasGetOutOfJailFreeCard = true;
                    manager.SetComponentData(player, jail);
                    return true;
                }
                return false;
            }

            var inventory = manager.GetComponentData<PlayerCardInventory>(player);
            if (inventory.HasPendingChoice) return false;
            if (inventory.Count < Capacity)
            {
                SetCard(ref inventory, inventory.Count, card);
                inventory.Count++;
                manager.SetComponentData(player, inventory);
                SyncLegacyJailFlag(manager, player, inventory);
                return true;
            }

            if (manager.HasComponent<AiOpponent>(player))
            {
                RemoveAt(ref inventory, 0);
                SetCard(ref inventory, inventory.Count, card);
                inventory.Count++;
                manager.SetComponentData(player, inventory);
                SyncLegacyJailFlag(manager, player, inventory);
                return true;
            }

            inventory.HasPendingChoice = true;
            inventory.PendingCard = (byte)card;
            manager.SetComponentData(player, inventory);
            return false;
        }

        public static bool ResolveOverflow(EntityManager manager, Entity player, byte discardSlot)
        {
            if (!manager.HasComponent<PlayerCardInventory>(player)) return false;
            var inventory = manager.GetComponentData<PlayerCardInventory>(player);
            if (!inventory.HasPendingChoice || inventory.Count != Capacity) return false;

            if (discardSlot == DiscardNewCard)
            {
                inventory.HasPendingChoice = false;
                inventory.PendingCard = (byte)PlayerCardType.None;
            }
            else
            {
                if (discardSlot >= inventory.Count) return false;
                RemoveAt(ref inventory, discardSlot);
                SetCard(ref inventory, inventory.Count, (PlayerCardType)inventory.PendingCard);
                inventory.Count++;
                inventory.HasPendingChoice = false;
                inventory.PendingCard = (byte)PlayerCardType.None;
            }

            manager.SetComponentData(player, inventory);
            SyncLegacyJailFlag(manager, player, inventory);
            return true;
        }

        public static bool Use(EntityManager manager, Entity player, byte slot)
        {
            if (!manager.HasComponent<PlayerCardInventory>(player) || !manager.HasComponent<JailState>(player)) return false;
            var inventory = manager.GetComponentData<PlayerCardInventory>(player);
            var jail = manager.GetComponentData<JailState>(player);
            if (inventory.HasPendingChoice || slot >= inventory.Count || !jail.InJail ||
                GetCard(inventory, slot) != PlayerCardType.GetOutOfJailFree) return false;

            RemoveAt(ref inventory, slot);
            jail.InJail = false;
            jail.TurnsInJail = 0;
            jail.DoublesRollAttempts = 0;
            jail.HasGetOutOfJailFreeCard = false;
            manager.SetComponentData(player, inventory);
            manager.SetComponentData(player, jail);
            SyncLegacyJailFlag(manager, player, inventory);
            return true;
        }

        public static bool UseMonopoly(EntityManager manager, Entity player, byte slot,
            PropertyColor color, EntityCommandBuffer feedback)
        {
            if (!manager.HasComponent<PlayerCardInventory>(player)) return false;
            var inventory = manager.GetComponentData<PlayerCardInventory>(player);
            if (inventory.HasPendingChoice || slot >= inventory.Count ||
                GetCard(inventory, slot) != PlayerCardType.Monopoly ||
                !ChanceCardRules.TryGetMonopolyPurchaseCost(manager, player, color, out var cost)) return false;

            int ownerId = manager.GetComponentData<GhostOwner>(player).NetworkId;
            var money = manager.GetComponentData<GhostMoneyComponet>(player);
            money.Value -= cost;
            manager.SetComponentData(player, money);

            using var query = manager.CreateEntityQuery(typeof(PropertySpaceTag), typeof(ColorCodeComponent),
                typeof(OwnerComponent), typeof(OwnerByEntityComponent));
            using var properties = query.ToEntityArray(Allocator.Temp);
            foreach (var property in properties)
            {
                if (manager.GetComponentData<ColorCodeComponent>(property).Value != color ||
                    manager.GetComponentData<OwnerComponent>(property).ID != PropertyConstants.Vacant) continue;
                manager.SetComponentData(property, new OwnerComponent { ID = ownerId });
                manager.SetComponentData(property, new OwnerByEntityComponent { Entity = player });
            }

            RemoveAt(ref inventory, slot);
            manager.SetComponentData(player, inventory);
            SyncLegacyJailFlag(manager, player, inventory);
            MoneyFeedback.Send(feedback, manager, player, -cost, MoneyChangeReason.Purchase);
            return true;
        }

        static void RemoveAt(ref PlayerCardInventory inventory, int slot)
        {
            for (int i = slot; i < inventory.Count - 1; i++)
                SetCard(ref inventory, i, GetCard(inventory, i + 1));
            if (inventory.Count > 0)
            {
                inventory.Count--;
                SetCard(ref inventory, inventory.Count, PlayerCardType.None);
            }
        }

        static void SyncLegacyJailFlag(EntityManager manager, Entity player, PlayerCardInventory inventory)
        {
            if (!manager.HasComponent<JailState>(player)) return;
            bool hasCard = false;
            for (int i = 0; i < inventory.Count; i++)
                if (GetCard(inventory, i) == PlayerCardType.GetOutOfJailFree) { hasCard = true; break; }
            var jail = manager.GetComponentData<JailState>(player);
            jail.HasGetOutOfJailFreeCard = hasCard;
            manager.SetComponentData(player, jail);
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateBefore(typeof(ChangeTurnSystem))]
    public partial struct CardInventoryRpcSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameStateComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var manager = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var active = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            var game = SystemAPI.GetSingleton<GameStateComponent>();

            foreach (var (request, _, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<UsePlayerCardRpc>>().WithEntityAccess())
            {
                var connection = request.ValueRO.SourceConnection;
                byte slot = SystemAPI.GetComponent<UsePlayerCardRpc>(entity).Slot;
                if (GameplayActionSource.Owns(manager, entity, connection, active) && game.State == GameState.Rolling)
                {
                    int color = SystemAPI.GetComponent<UsePlayerCardRpc>(entity).Color;
                    if (manager.HasComponent<PlayerCardInventory>(active) && slot < manager.GetComponentData<PlayerCardInventory>(active).Count &&
                        CardInventoryRules.GetCard(manager.GetComponentData<PlayerCardInventory>(active), slot) == PlayerCardType.Monopoly)
                        CardInventoryRules.UseMonopoly(manager, active, slot, (PropertyColor)color, ecb);
                    else
                        CardInventoryRules.Use(manager, active, slot);
                }
                ecb.DestroyEntity(entity);
            }

            foreach (var (request, rpc, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ResolveCardOverflowRpc>>().WithEntityAccess())
            {
                var connection = request.ValueRO.SourceConnection;
                if (GameplayActionSource.Owns(manager, entity, connection, active) && game.State == GameState.Landing)
                    CardInventoryRules.ResolveOverflow(manager, active, rpc.ValueRO.DiscardSlot);
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(manager);
            ecb.Dispose();
        }
    }
}

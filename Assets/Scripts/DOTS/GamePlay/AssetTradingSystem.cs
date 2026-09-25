using System;
using System.Collections.Generic;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    public enum AssetAction : byte { SellBuilding, BuyBuilding, Offer, Accept, Decline, Cancel, SellGroupBuildings }
    public struct AssetTradeRpc : IRpcCommand
    {
        public AssetAction Action;
        public int PropertyId, BuyerId, Price, OfferId;
    }
    public struct AssetTradeReplyRpc : IRpcCommand
    {
        public int OfferId, PropertyId, SellerId, BuyerId, Price;
        public bool Open;
        public FixedString128Bytes Message;
    }

    public static class AssetTradingRules
    {
        public static bool HasBuildings(EntityManager em, Entity property, NativeArray<Entity> properties)
        {
            var color = em.HasComponent<ColorCodeComponent>(property) ? em.GetComponentData<ColorCodeComponent>(property).Value : PropertyColor.None;
            foreach (var other in properties)
                if ((other == property || (color >= PropertyColor.Brown && em.HasComponent<ColorCodeComponent>(other) && em.GetComponentData<ColorCodeComponent>(other).Value == color)) &&
                    em.HasComponent<HouseCount>(other) && em.GetComponentData<HouseCount>(other).Value > 0) return true;
            return false;
        }

        // One level at a time preserves even building and selling across the group.
        public static string BuildingError(EntityManager em, Entity property, NativeArray<Entity> properties, bool buying)
        {
            if (!em.HasComponent<HouseCount>(property) || !em.HasComponent<HousePriceComponent>(property) ||
                em.GetComponentData<HousePriceComponent>(property).Value <= 0 || !em.HasComponent<ColorCodeComponent>(property)) return "This property cannot have buildings.";
            int level = em.GetComponentData<HouseCount>(property).Value;
            if (buying ? level >= 5 : level <= 0) return buying ? "This property already has a hotel." : "There are no buildings to sell.";
            var color = em.GetComponentData<ColorCodeComponent>(property).Value;
            if (color < PropertyColor.Brown) return "This property cannot have buildings.";
            int owner = em.GetComponentData<OwnerComponent>(property).ID, groupSize = 0, houses = 0, hotels = 0;
            foreach (var other in properties)
            {
                int n = em.HasComponent<HouseCount>(other) ? em.GetComponentData<HouseCount>(other).Value : 0;
                if (n == 5) hotels++; else houses += n;
                if (!em.HasComponent<ColorCodeComponent>(other) || em.GetComponentData<ColorCodeComponent>(other).Value != color) continue;
                groupSize++;
                if (buying && em.GetComponentData<OwnerComponent>(other).ID != owner) return "Own the entire color group before building.";
                if (other != property && (buying ? level > n : level < n)) return buying ? "Build evenly across this color group." : "Sell evenly: choose a property with the most buildings.";
            }
            if (buying && groupSize < 2) return "The color group is incomplete.";
            if (buying && level == 4 && hotels >= 12) return "The bank has no hotels available.";
            if ((buying && level < 4 && houses >= 32) || (!buying && level == 5 && houses > 28)) return "The bank does not have enough houses.";
            return null;
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateBefore(typeof(MonopolyTrackerSystem))]
    public partial class AssetTradingSystem : SystemBase
    {
        sealed class Offer
        {
            public int Id, Property, Seller, Buyer, Price;
            public double Expires;
        }
        readonly Dictionary<int, Offer> offers = new();
        int nextId;
        EntityQuery properties, players, connections, requests;
        protected override void OnCreate()
        {
            RequireForUpdate<GameStateComponent>();
            properties = GetEntityQuery(typeof(OwnerComponent), typeof(SpaceIDComponent));
            players = GetEntityQuery(typeof(GhostOwner), typeof(GhostMoneyComponet), typeof(BankruptPlayer));
            connections = GetEntityQuery(typeof(NetworkId));
            requests = GetEntityQuery(typeof(AssetTradeRpc), typeof(ReceiveRpcCommandRequest));
        }
        Entity Connection(int id)
        {
            using var all = connections.ToEntityArray(Allocator.Temp);
            foreach (var e in all) if (EntityManager.GetComponentData<NetworkId>(e).Value == id) return e;
            return Entity.Null;
        }
        Entity Player(int id)
        {
            using var all = players.ToEntityArray(Allocator.Temp);
            foreach (var e in all) if (EntityManager.GetComponentData<GhostOwner>(e).NetworkId == id && !EntityManager.GetComponentData<BankruptPlayer>(e).Value) return e;
            return Entity.Null;
        }
        Entity Property(int id, NativeArray<Entity> all)
        {
            foreach (var e in all) if (EntityManager.GetComponentData<SpaceIDComponent>(e).Value == id) return e;
            return Entity.Null;
        }
        void Reply(int id, string message, Offer offer = null, bool open = false)
        {
            var connection = Connection(id);
            if (connection == Entity.Null) return;
            var e = EntityManager.CreateEntity();
            EntityManager.AddComponentData(e, new AssetTradeReplyRpc { Message = message, Open = open,
                OfferId = offer?.Id ?? 0, PropertyId = offer?.Property ?? 0, SellerId = offer?.Seller ?? 0,
                BuyerId = offer?.Buyer ?? 0, Price = offer?.Price ?? 0 });
            EntityManager.AddComponentData(e, new SendRpcCommandRequest { TargetConnection = connection });
        }
        void Close(Offer offer, string message)
        {
            offers.Remove(offer.Id);
            Reply(offer.Seller, message, offer); Reply(offer.Buyer, message, offer);
        }
        protected override void OnUpdate()
        {
            using var moneyCommands = new EntityCommandBuffer(Allocator.Temp);
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            using var all = properties.ToEntityArray(Allocator.Temp);
            foreach (var offer in new List<Offer>(offers.Values))
            {
                var property = Property(offer.Property, all);
                if (game.State == GameState.GameOver || SystemAPI.Time.ElapsedTime >= offer.Expires ||
                    Player(offer.Seller) == Entity.Null || Player(offer.Buyer) == Entity.Null ||
                    Connection(offer.Seller) == Entity.Null || Connection(offer.Buyer) == Entity.Null || property == Entity.Null ||
                    EntityManager.GetComponentData<OwnerComponent>(property).ID != offer.Seller || AssetTradingRules.HasBuildings(EntityManager, property, all))
                    Close(offer, "Offer expired or the property is no longer available.");
            }
            using var rpcs = requests.ToEntityArray(Allocator.Temp);
            foreach (var entity in rpcs)
            {
                var rpc = EntityManager.GetComponentData<AssetTradeRpc>(entity);
                var source = EntityManager.GetComponentData<ReceiveRpcCommandRequest>(entity).SourceConnection;
                EntityManager.DestroyEntity(entity);
                if (!EntityManager.HasComponent<NetworkId>(source)) continue;
                int id = EntityManager.GetComponentData<NetworkId>(source).Value;
                var player = Player(id);
                if (player == Entity.Null || !game.AllPlacesInstantiated || game.State == GameState.GameOver) { Reply(id, "Trading is unavailable."); continue; }
                if (rpc.Action == AssetAction.Cancel || rpc.Action == AssetAction.Decline || rpc.Action == AssetAction.Accept)
                {
                    if (!offers.TryGetValue(rpc.OfferId, out var offer)) { Reply(id, "This offer is no longer available."); continue; }
                    if (rpc.Action == AssetAction.Cancel && id == offer.Seller) Close(offer, "Offer cancelled.");
                    else if (rpc.Action == AssetAction.Decline && id == offer.Buyer) Close(offer, "Offer declined.");
                    else if (rpc.Action == AssetAction.Accept && id == offer.Buyer)
                    {
                        var property = Property(offer.Property, all);
                        var seller = Player(offer.Seller);
                        if (seller == Entity.Null || property == Entity.Null || EntityManager.GetComponentData<OwnerComponent>(property).ID != offer.Seller || AssetTradingRules.HasBuildings(EntityManager, property, all)) { Close(offer, "This property can no longer be sold."); continue; }
                        var cash = EntityManager.GetComponentData<GhostMoneyComponet>(player);
                        var sellerCash = EntityManager.GetComponentData<GhostMoneyComponet>(seller);
                        if (cash.Value < offer.Price || (long)sellerCash.Value + offer.Price > int.MaxValue) { Reply(id, "There is not enough cash to complete this sale."); continue; }
                        cash.Value -= offer.Price; sellerCash.Value += offer.Price;
                        EntityManager.SetComponentData(player, cash); EntityManager.SetComponentData(seller, sellerCash);
                        EntityManager.SetComponentData(property, new OwnerComponent { ID = id });
                        EntityManager.SetComponentData(property, new OwnerByEntityComponent { Entity = player });
                        MoneyFeedback.Send(moneyCommands, EntityManager, seller, offer.Price, MoneyChangeReason.Trade, player, -offer.Price);
                        Close(offer, "Property sale completed.");
                    }
                    else Reply(id, "You cannot respond to this offer.");
                    continue;
                }
                var owned = Property(rpc.PropertyId, all);
                if (owned == Entity.Null || EntityManager.GetComponentData<OwnerComponent>(owned).ID != id) { Reply(id, "You do not own this property."); continue; }
                if (rpc.Action == AssetAction.Offer)
                {
                    if (rpc.BuyerId == id || Player(rpc.BuyerId) == Entity.Null || Connection(rpc.BuyerId) == Entity.Null || rpc.Price <= 0) { Reply(id, "Choose another player and enter a positive price."); continue; }
                    if (AssetTradingRules.HasBuildings(EntityManager, owned, all)) { Reply(id, "Sell all buildings in this color group before offering the property."); continue; }
                    foreach (var old in new List<Offer>(offers.Values)) if (old.Seller == id) Close(old, "Offer replaced by a new offer.");
                    var offer = new Offer { Id = ++nextId, Property = rpc.PropertyId, Seller = id, Buyer = rpc.BuyerId, Price = rpc.Price, Expires = SystemAPI.Time.ElapsedTime + 120 };
                    offers.Add(offer.Id, offer);
                    Reply(id, "Offer sent. Waiting for the buyer (expires in 2 minutes).", offer, true);
                    Reply(offer.Buyer, "You received a property offer (expires in 2 minutes).", offer, true);
                }
                else if (rpc.Action == AssetAction.SellGroupBuildings)
                {
                    if (!EntityManager.HasComponent<ColorCodeComponent>(owned)) continue;
                    var color = EntityManager.GetComponentData<ColorCodeComponent>(owned).Value;
                    if (color < PropertyColor.Brown) continue;
                    long refund = 0;
                    var group = new List<Entity>();
                    foreach (var p in all)
                    {
                        if (!EntityManager.HasComponent<ColorCodeComponent>(p) || EntityManager.GetComponentData<ColorCodeComponent>(p).Value != color ||
                            !EntityManager.HasComponent<HouseCount>(p) || EntityManager.GetComponentData<HouseCount>(p).Value <= 0) continue;
                        if (EntityManager.GetComponentData<OwnerComponent>(p).ID != id || !EntityManager.HasComponent<HousePriceComponent>(p)) { group.Clear(); break; }
                        group.Add(p);
                        refund += (long)EntityManager.GetComponentData<HouseCount>(p).Value * (EntityManager.GetComponentData<HousePriceComponent>(p).Value / 2);
                    }
                    var cash = EntityManager.GetComponentData<GhostMoneyComponet>(player);
                    if (group.Count == 0 || refund <= 0 || (long)cash.Value + refund > int.MaxValue) { Reply(id, "There are no group buildings available to sell."); continue; }
                    foreach (var p in group) EntityManager.SetComponentData(p, new HouseCount());
                    cash.Value += (int)refund; EntityManager.SetComponentData(player, cash);
                    MoneyFeedback.Send(moneyCommands, EntityManager, player, (int)refund, MoneyChangeReason.Building);
                    Reply(id, "All group buildings sold to the bank for half price.");
                }
                else if (rpc.Action == AssetAction.SellBuilding || rpc.Action == AssetAction.BuyBuilding)
                {
                    bool buying = rpc.Action == AssetAction.BuyBuilding;
                    var error = AssetTradingRules.BuildingError(EntityManager, owned, all, buying);
                    if (error != null) { Reply(id, error); continue; }
                    int cost = EntityManager.GetComponentData<HousePriceComponent>(owned).Value;
                    var cash = EntityManager.GetComponentData<GhostMoneyComponet>(player);
                    long balance = (long)cash.Value + (buying ? -cost : cost / 2);
                    if (balance < 0 || balance > int.MaxValue) { Reply(id, "There is not enough cash for this purchase."); continue; }
                    var level = EntityManager.GetComponentData<HouseCount>(owned);
                    level.Value += buying ? 1 : -1;
                    cash.Value = (int)balance;
                    EntityManager.SetComponentData(owned, level); EntityManager.SetComponentData(player, cash);
                    MoneyFeedback.Send(moneyCommands, EntityManager, player, buying ? -cost : cost / 2, MoneyChangeReason.Building);
                    Reply(id, buying ? "Building purchased." : "Building sold to the bank for half price.");
                }
            }
            moneyCommands.Playback(EntityManager);
        }
    }
}

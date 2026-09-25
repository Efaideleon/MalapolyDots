using System.Collections.Generic;
using System.Text;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GamePlay;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.Mediator
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class AssetPortfolioSystem : SystemBase
    {
        VisualElement root, rows, offerRows;
        Button launch;
        Label cashLabel, summary, status;
        readonly Queue<AssetTradeRpc> pending = new();
        readonly Dictionary<int, AssetTradeReplyRpc> offers = new();
        EntityQuery properties, players, replies;
        readonly Dictionary<int, int> draftPrices = new();
        readonly Dictionary<int, int> draftBuyers = new();
        string fingerprint;
        int localId;
        bool dirtyOffers;
        protected override void OnCreate()
        {
            RequireForUpdate<GameStateComponent>();
            RequireForUpdate<ForegroundContainterComponent>();
            RequireForUpdate<GameScreenInitializedFlag>();
            RequireForUpdate<NetworkId>();
            properties = GetEntityQuery(typeof(OwnerComponent), typeof(SpaceIDComponent), typeof(NameComponent));
            players = GetEntityQuery(typeof(GhostOwner), typeof(GhostMoneyComponet), typeof(BankruptPlayer), typeof(NameComponent));
            replies = GetEntityQuery(typeof(AssetTradeReplyRpc), typeof(ReceiveRpcCommandRequest));
        }
        void Open()
        {
            root.AddToClassList("is-open"); root.BringToFront(); fingerprint = null;
        }
        void Send(AssetTradeRpc rpc)
        {
            pending.Enqueue(rpc); status.text = "Waiting for the server…";
        }
        protected override void OnUpdate()
        {
            var foreground = SystemAPI.ManagedAPI.GetSingleton<ForegroundContainterComponent>().Value;
            if (foreground == null || !SystemAPI.GetSingleton<GameScreenInitializedFlag>().Value) return;
            if (root == null || root.parent != foreground)
            {
                Cleanup();
                var asset = Resources.Load<VisualTreeAsset>("UI/AssetPortfolio");
                if (asset == null) return;
                root = asset.CloneTree(); root.AddToClassList("portfolio-root"); foreground.Add(root);
                rows = root.Q("properties"); offerRows = root.Q("offers"); cashLabel = root.Q<Label>("cash");
                summary = root.Q<Label>("inventorySummary"); status = root.Q<Label>("tradeStatus");
                root.Q<Button>("closePortfolio").clicked += () => root.RemoveFromClassList("is-open");
                var documentRoot = foreground;
                while (documentRoot.parent != null) documentRoot = documentRoot.parent;
                launch = documentRoot.Q<Button>("buySellButton");
                if (launch != null) { launch.clicked += Open; launch.tooltip = "View your properties, build, and sell"; }
            }
            localId = SystemAPI.GetSingleton<NetworkId>().Value;
            using (var received = replies.ToEntityArray(Allocator.Temp))
                foreach (var e in received)
                {
                    var reply = EntityManager.GetComponentData<AssetTradeReplyRpc>(e);
                    status.text = reply.Message.ToString();
                    if (reply.OfferId != 0)
                    {
                        if (reply.Open) offers[reply.OfferId] = reply; else offers.Remove(reply.OfferId);
                        dirtyOffers = true;
                        if (reply.Open && reply.BuyerId == localId) Open();
                    }
                    EntityManager.DestroyEntity(e);
                }
            while (pending.Count > 0)
            {
                var e = EntityManager.CreateEntity();
                EntityManager.AddComponentData(e, pending.Dequeue());
                EntityManager.AddComponent<SendRpcCommandRequest>(e);
            }
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            if (game.State == GameState.GameOver) { root.RemoveFromClassList("is-open"); launch?.SetEnabled(false); return; }
            launch?.SetEnabled(true);
            if (!root.ClassListContains("is-open")) return;
            using var all = properties.ToEntityArray(Allocator.Temp);
            using var people = players.ToEntityArray(Allocator.Temp);
            var buyers = new List<(int id, string name)>();
            int cash = 0; bool bankrupt = true;
            var signature = new StringBuilder();
            foreach (var person in people)
            {
                int id = EntityManager.GetComponentData<GhostOwner>(person).NetworkId;
                bool eliminated = EntityManager.GetComponentData<BankruptPlayer>(person).Value;
                if (id == localId) { cash = EntityManager.GetComponentData<GhostMoneyComponet>(person).Value; bankrupt = eliminated; }
                else if (!eliminated) buyers.Add((id, EntityManager.GetComponentData<NameComponent>(person).Value.ToString()));
                signature.Append(id).Append(eliminated);
            }
            buyers.Sort((a,b) => a.id.CompareTo(b.id));
            signature.Append(cash).Append(bankrupt);
            foreach (var p in all)
                signature.Append('|').Append(EntityManager.GetComponentData<SpaceIDComponent>(p).Value).Append(':')
                    .Append(EntityManager.GetComponentData<OwnerComponent>(p).ID).Append(':').Append(Level(p));
            if (dirtyOffers) { RenderOffers(all, people); dirtyOffers = false; }
            if (fingerprint == signature.ToString()) return;
            fingerprint = signature.ToString();
            cashLabel.text = $"Cash  ${cash:N0}";
            rows.Clear(); int ownedCount = 0, houses = 0, hotels = 0;
            var sorted = new List<Entity>();
            foreach (var p in all) if (EntityManager.GetComponentData<OwnerComponent>(p).ID == localId) sorted.Add(p);
            sorted.Sort((a,b) => EntityManager.GetComponentData<SpaceIDComponent>(a).Value.CompareTo(EntityManager.GetComponentData<SpaceIDComponent>(b).Value));
            foreach (var p in sorted)
            {
                ownedCount++; int level = Level(p); if (level == 5) hotels++; else houses += level;
                int propertyId = EntityManager.GetComponentData<SpaceIDComponent>(p).Value;
                int cost = EntityManager.HasComponent<HousePriceComponent>(p) ? EntityManager.GetComponentData<HousePriceComponent>(p).Value : 0;
                int value = EntityManager.HasComponent<GhostPriceComponent>(p) ? EntityManager.GetComponentData<GhostPriceComponent>(p).Value : 0;
                var row = new VisualElement(); row.AddToClassList("portfolio-row"); rows.Add(row);
                AddLabel(row, EntityManager.GetComponentData<NameComponent>(p).Value.ToString(), "portfolio-property-name");
                string color = EntityManager.HasComponent<ColorCodeComponent>(p) ? EntityManager.GetComponentData<ColorCodeComponent>(p).Value.ToString() : "Property";
                AddLabel(row, $"{color} · Deed value ${value:N0} · {(level == 5 ? "1 hotel" : level + " houses")}", "portfolio-details");
                var actions = new VisualElement(); actions.AddToClassList("portfolio-actions"); row.Add(actions);
                if (cost > 0)
                {
                    string buyError = AssetTradingRules.BuildingError(EntityManager, p, all, true);
                    string sellError = AssetTradingRules.BuildingError(EntityManager, p, all, false);
                    var buy = AddButton(actions, $"Buy {(level == 4 ? "hotel" : "house")} · ${cost:N0}", () => Send(new AssetTradeRpc { Action = AssetAction.BuyBuilding, PropertyId = propertyId }));
                    buy.SetEnabled(!bankrupt && buyError == null && cash >= cost); buy.tooltip = buyError ?? "Build one level";
                    if (level > 0)
                    {
                        var sell = AddButton(actions, $"{(level == 5 ? "Hotel → 4 houses" : "Sell 1 house")} · +${cost / 2:N0}", () => Send(new AssetTradeRpc { Action = AssetAction.SellBuilding, PropertyId = propertyId }));
                        sell.SetEnabled(!bankrupt && sellError == null); sell.tooltip = sellError ?? "Sell to the bank for half price";
                        long refund = 0;
                        var groupColor = EntityManager.GetComponentData<ColorCodeComponent>(p).Value;
                        foreach (var member in all)
                            if (EntityManager.HasComponent<ColorCodeComponent>(member) && EntityManager.GetComponentData<ColorCodeComponent>(member).Value == groupColor &&
                                EntityManager.GetComponentData<OwnerComponent>(member).ID == localId && EntityManager.HasComponent<HousePriceComponent>(member))
                                refund += (long)Level(member) * (EntityManager.GetComponentData<HousePriceComponent>(member).Value / 2);
                        var sellGroup = AddButton(actions, $"Sell ALL group buildings · +${refund:N0}", () => Send(new AssetTradeRpc { Action = AssetAction.SellGroupBuildings, PropertyId = propertyId }));
                        sellGroup.SetEnabled(!bankrupt);
                        if (sellError != null) AddLabel(row, sellError, "portfolio-help");
                    }
                }
                if (AssetTradingRules.HasBuildings(EntityManager, p, all))
                    AddLabel(row, "Property sale locked: sell every building in this color group first.", "portfolio-help");
                else if (buyers.Count == 0) AddLabel(row, "No other players available to buy this property.", "portfolio-help");
                else
                {
                    var trade = new VisualElement(); trade.AddToClassList("portfolio-actions"); row.Add(trade);
                    var names = buyers.ConvertAll(b => b.name + " (Player " + b.id + ")");
                    int buyerIndex = draftBuyers.TryGetValue(propertyId, out var selectedBuyer) ? buyers.FindIndex(b => b.id == selectedBuyer) : 0;
                    var buyer = new DropdownField("Buyer", names, System.Math.Max(0, buyerIndex)) { tooltip = "Choose the buyer" }; trade.Add(buyer);
                    buyer.RegisterValueChangedCallback(evt => draftBuyers[propertyId] = buyers[buyer.index].id);
                    var price = new IntegerField("Price $") { value = draftPrices.TryGetValue(propertyId, out var draftPrice) ? draftPrice : value > 0 ? value : 1, tooltip = "Sale price in dollars" }; trade.Add(price);
                    var offer = AddButton(trade, "Offer property", () => Send(new AssetTradeRpc { Action = AssetAction.Offer, PropertyId = propertyId, BuyerId = buyers[buyer.index].id, Price = price.value }));
                    offer.SetEnabled(!bankrupt && price.value > 0);
                    price.RegisterValueChangedCallback(evt => { draftPrices[propertyId] = evt.newValue; offer.SetEnabled(!bankrupt && evt.newValue > 0); });
                }
            }
            summary.text = $"{ownedCount} properties   ·   {houses} houses   ·   {hotels} hotels";
            if (ownedCount == 0) AddLabel(rows, "You do not own any properties yet. Buy an unowned property when you land on it.", "portfolio-help");
        }
        int Level(Entity property) => EntityManager.HasComponent<HouseCount>(property) ? EntityManager.GetComponentData<HouseCount>(property).Value : 0;
        void RenderOffers(NativeArray<Entity> all, NativeArray<Entity> people)
        {
            offerRows.Clear();
            foreach (var offer in offers.Values)
            {
                var copy = offer;
                string name = "Property " + offer.PropertyId, other = "Player " + (offer.BuyerId == localId ? offer.SellerId : offer.BuyerId);
                foreach (var p in all) if (EntityManager.GetComponentData<SpaceIDComponent>(p).Value == offer.PropertyId) name = EntityManager.GetComponentData<NameComponent>(p).Value.ToString();
                foreach (var p in people) if (EntityManager.GetComponentData<GhostOwner>(p).NetworkId == (offer.BuyerId == localId ? offer.SellerId : offer.BuyerId)) other = EntityManager.GetComponentData<NameComponent>(p).Value.ToString();
                var row = new VisualElement(); row.AddToClassList("portfolio-row"); row.AddToClassList("portfolio-offer"); offerRows.Add(row);
                AddLabel(row, $"{name} · ${offer.Price:N0}", "portfolio-property-name");
                AddLabel(row, offer.BuyerId == localId ? $"{other} offers to sell you this property." : $"Waiting for {other} to accept. Sending another offer replaces this one.", "portfolio-help");
                var actions = new VisualElement(); actions.AddToClassList("portfolio-actions"); row.Add(actions);
                if (offer.BuyerId == localId)
                {
                    AddButton(actions, "Accept & pay", () => Send(new AssetTradeRpc { Action = AssetAction.Accept, OfferId = copy.OfferId }));
                    AddButton(actions, "Decline", () => Send(new AssetTradeRpc { Action = AssetAction.Decline, OfferId = copy.OfferId }));
                }
                else AddButton(actions, "Cancel offer", () => Send(new AssetTradeRpc { Action = AssetAction.Cancel, OfferId = copy.OfferId }));
            }
        }
        static void AddLabel(VisualElement parent, string text, string className)
        {
            var label = new Label(text) { enableRichText = false }; label.AddToClassList(className); parent.Add(label);
        }
        static Button AddButton(VisualElement parent, string text, System.Action action)
        {
            var button = new Button(action) { text = text }; parent.Add(button); return button;
        }
        void Cleanup()
        {
            if (launch != null) launch.clicked -= Open;
            root?.RemoveFromHierarchy(); root = null; launch = null; fingerprint = null;
        }
        protected override void OnStopRunning() { Cleanup(); pending.Clear(); offers.Clear(); draftPrices.Clear(); draftBuyers.Clear(); }
        protected override void OnDestroy() => Cleanup();
    }
}

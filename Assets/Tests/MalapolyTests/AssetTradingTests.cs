using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GamePlay;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public class AssetTradingTests
{
    World world;
    EntityManager em;
    AssetTradingSystem system;
    Entity seller, buyer, first, second, connection1, connection2;
    [SetUp] public void Setup()
    {
        world = new World("Asset trading tests"); em = world.EntityManager;
        system = world.GetOrCreateSystemManaged<AssetTradingSystem>();
        var game = em.CreateEntity(typeof(GameStateComponent));
        em.SetComponentData(game, new GameStateComponent { AllPlacesInstantiated = true, State = GameState.Landing });
        seller = Player(1, 100); buyer = Player(2, 500);
        connection1 = Connection(1); connection2 = Connection(2);
        first = Property(1, 0); second = Property(2, 0);
    }
    [TearDown] public void Cleanup() => world.Dispose();
    Entity Player(int id, int cash)
    {
        var e = em.CreateEntity(typeof(GhostOwner), typeof(GhostMoneyComponet), typeof(BankruptPlayer));
        em.SetComponentData(e, new GhostOwner { NetworkId = id }); em.SetComponentData(e, new GhostMoneyComponet { Value = cash }); return e;
    }
    Entity Connection(int id) { var e = em.CreateEntity(typeof(NetworkId)); em.SetComponentData(e, new NetworkId { Value = id }); return e; }
    Entity Property(int id, int level)
    {
        var e = em.CreateEntity(typeof(SpaceIDComponent), typeof(OwnerComponent), typeof(OwnerByEntityComponent), typeof(HouseCount), typeof(HousePriceComponent), typeof(ColorCodeComponent));
        em.SetComponentData(e, new SpaceIDComponent { Value = id }); em.SetComponentData(e, new OwnerComponent { ID = 1 });
        em.SetComponentData(e, new OwnerByEntityComponent { Entity = seller }); em.SetComponentData(e, new HouseCount { Value = level });
        em.SetComponentData(e, new HousePriceComponent { Value = 50 }); em.SetComponentData(e, new ColorCodeComponent { Value = PropertyColor.Brown }); return e;
    }
    void Send(AssetAction action, Entity connection, int property = 1, int price = 200, int offerId = 1)
    {
        var e = em.CreateEntity(typeof(AssetTradeRpc), typeof(ReceiveRpcCommandRequest));
        em.SetComponentData(e, new AssetTradeRpc { Action = action, PropertyId = property, BuyerId = 2, Price = price, OfferId = offerId });
        em.SetComponentData(e, new ReceiveRpcCommandRequest { SourceConnection = connection }); system.Update();
    }
    int Cash(Entity e) => em.GetComponentData<GhostMoneyComponet>(e).Value;
    int Level(Entity e) => em.GetComponentData<HouseCount>(e).Value;
    void Buildings(int a, int b) { em.SetComponentData(first, new HouseCount { Value = a }); em.SetComponentData(second, new HouseCount { Value = b }); }
    [Test] public void HouseSaleReturnsHalfAndRemovesOneHouse()
    {
        Buildings(2, 1); Send(AssetAction.SellBuilding, connection1);
        Assert.AreEqual(1, Level(first)); Assert.AreEqual(125, Cash(seller));
    }
    [Test] public void HotelSaleReturnsFourHousesAndHalfUpgradePrice()
    {
        Buildings(5, 5); Send(AssetAction.SellBuilding, connection1);
        Assert.AreEqual(4, Level(first)); Assert.AreEqual(125, Cash(seller));
    }
    [Test] public void CannotSellUnevenly()
    {
        Buildings(1, 2); Send(AssetAction.SellBuilding, connection1);
        Assert.AreEqual(1, Level(first)); Assert.AreEqual(100, Cash(seller));
    }
    [Test] public void NonOwnerCannotSellBuildings()
    {
        Buildings(2, 1); Send(AssetAction.SellBuilding, connection2);
        Assert.AreEqual(2, Level(first)); Assert.AreEqual(500, Cash(buyer));
    }
    [Test] public void PropertyTransfersOnlyAfterBuyerAcceptsAndOnlyOnce()
    {
        Send(AssetAction.Offer, connection1);
        Assert.AreEqual(1, em.GetComponentData<OwnerComponent>(first).ID); Assert.AreEqual(500, Cash(buyer));
        Send(AssetAction.Accept, connection1); Assert.AreEqual(500, Cash(buyer));
        Send(AssetAction.Accept, connection2); Send(AssetAction.Accept, connection2);
        Assert.AreEqual(2, em.GetComponentData<OwnerComponent>(first).ID);
        Assert.AreEqual(buyer, em.GetComponentData<OwnerByEntityComponent>(first).Entity);
        Assert.AreEqual(300, Cash(buyer)); Assert.AreEqual(300, Cash(seller));
    }
    [Test] public void BuildingsElsewhereInColorGroupBlockPropertySale()
    {
        Buildings(0, 1); Send(AssetAction.Offer, connection1); Send(AssetAction.Accept, connection2);
        Assert.AreEqual(1, em.GetComponentData<OwnerComponent>(first).ID); Assert.AreEqual(500, Cash(buyer));
    }
    [Test] public void InsufficientFundsCannotAccept()
    {
        Send(AssetAction.Offer, connection1, price: 501); Send(AssetAction.Accept, connection2);
        Assert.AreEqual(1, em.GetComponentData<OwnerComponent>(first).ID); Assert.AreEqual(100, Cash(seller));
    }
    [Test] public void DeclinedAndCancelledOffersCannotBeAccepted()
    {
        Send(AssetAction.Offer, connection1); Send(AssetAction.Decline, connection2); Send(AssetAction.Accept, connection2);
        Send(AssetAction.Offer, connection1); Send(AssetAction.Cancel, connection1, offerId: 2); Send(AssetAction.Accept, connection2, offerId: 2);
        Assert.AreEqual(1, em.GetComponentData<OwnerComponent>(first).ID); Assert.AreEqual(500, Cash(buyer));
    }
    [Test] public void BuildingAfterOfferInvalidatesIt()
    {
        Send(AssetAction.Offer, connection1); Send(AssetAction.BuyBuilding, connection1); Send(AssetAction.Accept, connection2);
        Assert.AreEqual(1, em.GetComponentData<OwnerComponent>(first).ID); Assert.AreEqual(500, Cash(buyer));
    }
    [Test] public void BuildingPurchaseMustBeEven()
    {
        Buildings(1, 0); Send(AssetAction.BuyBuilding, connection1);
        Assert.AreEqual(1, Level(first)); Assert.AreEqual(100, Cash(seller));
    }
    [Test] public void SellEntireGroupRefundsHotelAndAllExchangedHouses()
    {
        Buildings(5, 5); Send(AssetAction.SellGroupBuildings, connection1);
        Assert.AreEqual(0, Level(first)); Assert.AreEqual(0, Level(second)); Assert.AreEqual(350, Cash(seller));
    }
    [Test] public void HotelDowngradeNeedsFourBankHouses()
    {
        Buildings(5, 5);
        for (int i = 0; i < 8; i++) { var p = Property(10 + i, 4); em.SetComponentData(p, new ColorCodeComponent { Value = PropertyColor.Green }); }
        Send(AssetAction.SellBuilding, connection1);
        Assert.AreEqual(5, Level(first)); Assert.AreEqual(100, Cash(seller));
    }
    [Test]
    public void CompletedTradePresentsSellerWhoStartedOfferBeforeBuyer()
    {
        Send(AssetAction.Offer, connection1);
        using var messages = em.CreateEntityQuery(typeof(MoneyFeedbackRpc));
        Assert.AreEqual(0, messages.CalculateEntityCount(), "An offer alone must not show money moving.");
        Send(AssetAction.Accept, connection2);
        Assert.AreEqual(1, messages.CalculateEntityCount());
        var feedback = messages.GetSingleton<MoneyFeedbackRpc>();
        Assert.AreEqual(1, feedback.FirstPlayerId);
        Assert.AreEqual(200, feedback.FirstDelta);
        Assert.AreEqual(2, feedback.SecondPlayerId);
        Assert.AreEqual(-200, feedback.SecondDelta);
        Assert.AreEqual(MoneyChangeReason.Trade, feedback.Reason);
        Send(AssetAction.Accept, connection2);
        Assert.AreEqual(1, messages.CalculateEntityCount(), "A repeated accept must not replay the transaction.");
    }

    [Test]
    public void DeclinedTradeDoesNotPresentMoneyChanges()
    {
        Send(AssetAction.Offer, connection1);
        Send(AssetAction.Decline, connection2);
        using var messages = em.CreateEntityQuery(typeof(MoneyFeedbackRpc));
        Assert.AreEqual(0, messages.CalculateEntityCount());
    }
}

using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using NUnit.Framework;
using Unity.Entities;
using Unity.NetCode;

public class BuyHouseSystemTest
{
    private World world;
    private EntityManager manager;
    private Entity player, connection, property, sibling, gameState;

    [SetUp]
    public void Setup()
    {
        world = new World("House purchase tests");
        manager = world.EntityManager;
        var group = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
        group.AddSystemToUpdateList(world.CreateSystem<MonopolyTrackerSystem>());
        group.AddSystemToUpdateList(world.CreateSystem<BuyHouseSystem>());
        group.AddSystemToUpdateList(world.CreateSystem<RentCalculatorSystem>());
        group.SortSystems();
        player = manager.CreateEntity(typeof(GhostOwner), typeof(GhostMoneyComponet));
        manager.SetComponentData(player, new GhostOwner { NetworkId = 7 });
        manager.SetComponentData(player, new GhostMoneyComponet { Value = 100 });
        connection = manager.CreateEntity(typeof(NetworkId));
        manager.SetComponentData(connection, new NetworkId { Value = 7 });
        manager.SetComponentData(manager.CreateEntity(typeof(CurrentActivePlayer)), new CurrentActivePlayer { Entity = player });
        gameState = manager.CreateEntity(typeof(GameStateComponent));
        manager.SetComponentData(gameState, new GameStateComponent { State = GameState.Landing, AllPlacesInstantiated = true });
        property = CreateProperty(1);
        sibling = CreateProperty(2);
    }

    private Entity CreateProperty(int id)
    {
        var entity = manager.CreateEntity(typeof(PropertySpaceTag), typeof(SpaceIDComponent), typeof(OwnerComponent),
            typeof(ColorCodeComponent), typeof(MonopolyFlagComponent), typeof(HouseCount), typeof(HousePriceComponent), typeof(GhostRentComponent));
        manager.SetComponentData(entity, new SpaceIDComponent { Value = id });
        manager.SetComponentData(entity, new OwnerComponent { ID = 7 });
        manager.SetComponentData(entity, new ColorCodeComponent { Value = PropertyColor.Brown });
        manager.SetComponentData(entity, new HousePriceComponent { Value = 10 });
        var rents = manager.AddBuffer<BaseRentBuffer>(entity);
        for (int i = 0; i <= 5; i++) rents.Add(new BaseRentBuffer { Value = 5 + i * 10 });
        return entity;
    }

    private void Request(int count)
    {
        var entity = manager.CreateEntity(typeof(BuyHouseRpc), typeof(ReceiveRpcCommandRequest));
        manager.SetComponentData(entity, new BuyHouseRpc { PropertyId = 1, Count = count });
        manager.SetComponentData(entity, new ReceiveRpcCommandRequest { SourceConnection = connection });
    }

    [TearDown]
    public void TearDown() => world.Dispose();

    [Test]
    public void BuyingFinalPropertyUnlocksHousesAndUpdatesMoneyAndRent()
    {
        manager.SetComponentData(sibling, new OwnerComponent { ID = PropertyConstants.Vacant });
        Request(1);
        world.Update();
        Assert.AreEqual(0, manager.GetComponentData<HouseCount>(property).Value);
        manager.SetComponentData(sibling, new OwnerComponent { ID = 7 });
        Request(2);
        world.Update();
        Assert.IsTrue(manager.GetComponentData<MonopolyFlagComponent>(property).Value);
        Assert.IsTrue(manager.GetComponentData<MonopolyFlagComponent>(sibling).Value);
        Assert.AreEqual(2, manager.GetComponentData<HouseCount>(property).Value);
        Assert.AreEqual(80, manager.GetComponentData<GhostMoneyComponet>(player).Value);
        Assert.AreEqual(25, manager.GetComponentData<GhostRentComponent>(property).Value);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(5)]
    [TestCase(int.MaxValue)]
    public void InvalidCountsAreRejected(int count)
    {
        Request(count);
        world.Update();
        AssertUnchanged();
    }

    [Test]
    public void MonopolyDoublesOnlyUnimprovedRent()
    {
        world.Update();
        Assert.AreEqual(10, manager.GetComponentData<GhostRentComponent>(property).Value);
        Request(1);
        world.Update();
        Assert.AreEqual(15, manager.GetComponentData<GhostRentComponent>(property).Value);
    }

    [Test]
    public void CannotBuildOnAnotherPlayersGroup()
    {
        manager.SetComponentData(property, new OwnerComponent { ID = 8 });
        manager.SetComponentData(sibling, new OwnerComponent { ID = 8 });
        Request(1);
        world.Update();
        AssertUnchanged();
    }

    [Test]
    public void CannotBuyOnAnotherPlayersTurn()
    {
        manager.SetComponentData(connection, new NetworkId { Value = 8 });
        Request(1);
        world.Update();
        AssertUnchanged();
    }

    [Test]
    public void FourHousesUpgradeToOneHotelAndCannotBuildBeyondHotel()
    {
        Request(4);
        Request(1);
        world.Update();
        Assert.AreEqual(5, manager.GetComponentData<HouseCount>(property).Value);
        Assert.AreEqual(50, manager.GetComponentData<GhostMoneyComponet>(player).Value);
        Assert.AreEqual(55, manager.GetComponentData<GhostRentComponent>(property).Value);
        Request(1);
        world.Update();
        Assert.AreEqual(5, manager.GetComponentData<HouseCount>(property).Value);
        Assert.AreEqual(50, manager.GetComponentData<GhostMoneyComponet>(player).Value);
    }

    [Test]
    public void UnaffordableBatchIsRejectedWithoutPartialPurchase()
    {
        manager.SetComponentData(property, new HousePriceComponent { Value = 60 });
        Request(2);
        world.Update();
        AssertUnchanged();
    }

    [Test]
    public void LosingGroupOwnershipLocksBuildingAgain()
    {
        world.Update();
        manager.SetComponentData(sibling, new OwnerComponent { ID = 8 });
        Request(1);
        world.Update();
        Assert.IsFalse(manager.GetComponentData<MonopolyFlagComponent>(property).Value);
        AssertUnchanged();
    }

    [Test]
    public void WhitePropertiesDoNotUnlockHouses()
    {
        manager.SetComponentData(property, new ColorCodeComponent { Value = PropertyColor.White });
        manager.SetComponentData(sibling, new ColorCodeComponent { Value = PropertyColor.White });
        Request(1);
        world.Update();
        AssertUnchanged();
    }

#if UNITY_EDITOR
    [Test]
    public void BuyingCabuzAndInterseccionFormsTheBoardsFirstMonopoly()
    {
        var cabuz = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadMainAssetAtPath(
            "Assets/ScriptableObjects/SpacesData/Properties/Cabuz.asset"));
        var interseccion = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadMainAssetAtPath(
            "Assets/ScriptableObjects/SpacesData/Properties/Interseccion.asset"));
        var cabuzColor = (PropertyColor)cabuz.FindProperty("Color").intValue;
        var interseccionColor = (PropertyColor)interseccion.FindProperty("Color").intValue;
        Assert.AreEqual(PropertyColor.Brown, cabuzColor);
        Assert.AreEqual(cabuzColor, interseccionColor);
        manager.SetComponentData(property, new ColorCodeComponent { Value = cabuzColor });
        manager.SetComponentData(sibling, new ColorCodeComponent { Value = interseccionColor });
        Request(1);
        world.Update();
        Assert.IsTrue(manager.GetComponentData<MonopolyFlagComponent>(property).Value);
        Assert.IsTrue(manager.GetComponentData<MonopolyFlagComponent>(sibling).Value);
        Assert.AreEqual(1, manager.GetComponentData<HouseCount>(property).Value);
    }
#endif

    private void AssertUnchanged()
    {
        Assert.AreEqual(0, manager.GetComponentData<HouseCount>(property).Value);
        Assert.AreEqual(100, manager.GetComponentData<GhostMoneyComponet>(player).Value);
    }
}

public class HousePurchaseButtonTests
{
    [Test]
    public void ShowingPanelDoesNotUnlockHousesUntilMonopolyIsAvailable()
    {
        var root = new UnityEngine.UIElements.VisualElement();
        var actions = new UnityEngine.UIElements.VisualElement { name = "SpaceActions" };
        root.Add(actions);
        foreach (var definition in DOTS.UI.Panels.SpaceActionsPanelData.ButtonData)
        {
            if (definition.ContainerUXMLClassName != null)
                actions.Add(new UnityEngine.UIElements.VisualElement { name = definition.ContainerUXMLClassName });
            if (definition.ButtonUXMLClassName != null)
                actions.Add(new UnityEngine.UIElements.Button { name = definition.ButtonUXMLClassName });
        }
        var panel = new DOTS.UI.Panels.SpaceActionsPanel(root);
        var button = panel.ButtonSet[DOTS.UI.Panels.SpaceActionButtonsEnum.BuyHouse].Button;
        panel.Show();
        Assert.IsFalse(button.enabledSelf);
        panel.SetHousePurchaseAvailability(true);
        Assert.IsTrue(button.enabledSelf);
        panel.Hide();
        Assert.IsFalse(button.enabledSelf);
        panel.Show();
        Assert.IsTrue(button.enabledSelf);
        panel.SetHousePurchaseAvailability(false);
        panel.Show(); // An animation completing must not re-enable an ineligible button.
        Assert.IsFalse(button.enabledSelf);
    }
}

public class EconomyRulesTests
{
    [Test]
    public void RemotePlayerPaysConfiguredTaxOnlyOncePerLanding()
    {
        using var world = new World("Remote tax tests");
        var manager = world.EntityManager;
        var tax = manager.CreateEntity(typeof(TaxAmountComponent));
        manager.SetComponentData(tax, new TaxAmountComponent { Value = 200 });
        var player = manager.CreateEntity(typeof(GhostOwner), typeof(GhostMoneyComponet), typeof(SpaceLandedOn), typeof(LandingPaymentResolved));
        manager.SetComponentData(player, new GhostOwner { NetworkId = 7 });
        manager.SetComponentData(player, new GhostMoneyComponet { Value = 1500 });
        manager.SetComponentData(player, new SpaceLandedOn { entity = tax });
        manager.SetComponentData(manager.CreateEntity(typeof(CurrentActivePlayer)), new CurrentActivePlayer { Entity = player });
        manager.SetComponentData(manager.CreateEntity(typeof(GameStateComponent)), new GameStateComponent { State = GameState.Landing });
        var connection = manager.CreateEntity(typeof(NetworkId));
        manager.SetComponentData(connection, new NetworkId { Value = 7 });
        for (int i = 0; i < 2; i++)
        {
            var request = manager.CreateEntity(typeof(Assets.Scripts.DOTS.Mediator.PayTaxesRpc), typeof(ReceiveRpcCommandRequest));
            manager.SetComponentData(request, new ReceiveRpcCommandRequest { SourceConnection = connection });
        }
        world.GetOrCreateSystemManaged<SimulationSystemGroup>().AddSystemToUpdateList(world.CreateSystem<PayTaxesSystem>());
        world.Update();
        Assert.AreEqual(1300, manager.GetComponentData<GhostMoneyComponet>(player).Value);
        Assert.IsTrue(manager.GetComponentData<LandingPaymentResolved>(player).Value);
    }

    [Test]
    public void NegativeCashEliminatesPlayerAndLastSolventPlayerWins()
    {
        using var world = new World("Bankruptcy tests");
        var manager = world.EntityManager;
        var game = manager.CreateEntity(typeof(GameStateComponent));
        manager.SetComponentData(game, new GameStateComponent { State = GameState.Landing, AllPlacesInstantiated = true });
        var debtor = manager.CreateEntity(typeof(GhostMoneyComponet), typeof(BankruptPlayer), typeof(GhostOwner));
        manager.SetComponentData(debtor, new GhostMoneyComponet { Value = -1 });
        manager.SetComponentData(debtor, new GhostOwner { NetworkId = 1 });
        var survivor = manager.CreateEntity(typeof(GhostMoneyComponet), typeof(BankruptPlayer), typeof(GhostOwner));
        manager.SetComponentData(survivor, new GhostMoneyComponet { Value = 0 });
        manager.SetComponentData(survivor, new GhostOwner { NetworkId = 2 });
        var property = manager.CreateEntity(typeof(OwnerComponent), typeof(OwnerByEntityComponent), typeof(HouseCount), typeof(GhostRentComponent));
        manager.SetComponentData(property, new OwnerComponent { ID = 1 });
        manager.SetComponentData(property, new OwnerByEntityComponent { Entity = debtor });
        manager.SetComponentData(property, new HouseCount { Value = 5 });
        manager.SetComponentData(property, new GhostRentComponent { Value = 2000 });
        var group = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
        group.AddSystemToUpdateList(world.CreateSystem<BankruptcySystem>());
        world.Update();
        Assert.IsTrue(manager.GetComponentData<BankruptPlayer>(debtor).Value);
        Assert.IsFalse(manager.GetComponentData<BankruptPlayer>(survivor).Value, "Zero cash alone is not bankruptcy.");
        Assert.AreEqual(GameState.GameOver, manager.GetComponentData<GameStateComponent>(game).State);
        Assert.AreEqual(2, manager.GetComponentData<GameStateComponent>(game).WinnerNetworkId);
        Assert.AreEqual(PropertyConstants.Vacant, manager.GetComponentData<OwnerComponent>(property).ID);
        Assert.AreEqual(0, manager.GetComponentData<HouseCount>(property).Value);
        Assert.AreEqual(0, manager.GetComponentData<GhostRentComponent>(property).Value);
    }

    [TestCase(PropertyRentKind.Transport, 4, 7, 200)]
    [TestCase(PropertyRentKind.Transport, 1, 7, 25)]
    [TestCase(PropertyRentKind.Utility, 1, 7, 28)]
    [TestCase(PropertyRentKind.Utility, 2, 7, 70)]
    public void SpecialPropertyRentUsesOwnerPortfolioAndDice(PropertyRentKind kind, int count, int dice, int expected)
    {
        using var world = new World("Special rents");
        var manager = world.EntityManager;
        manager.SetComponentData(manager.CreateEntity(typeof(RollAmountComponent)), new RollAmountComponent { Value = dice });
        Entity first = Entity.Null;
        for (int i = 0; i < count; i++)
        {
            var property = manager.CreateEntity(typeof(OwnerComponent), typeof(GhostRentComponent), typeof(PropertyRentKindComponent));
            if (i == 0) first = property;
            manager.SetComponentData(property, new OwnerComponent { ID = 1 });
            manager.SetComponentData(property, new PropertyRentKindComponent { Value = kind });
            var rent = manager.AddBuffer<BaseRentBuffer>(property);
            foreach (int value in kind == PropertyRentKind.Utility ? new[] { 4, 10 } : new[] { 25, 50, 100, 200 })
                rent.Add(new BaseRentBuffer { Value = value });
        }
        world.GetOrCreateSystemManaged<SimulationSystemGroup>().AddSystemToUpdateList(world.CreateSystem<RentCalculatorSystem>());
        world.Update();
        Assert.AreEqual(expected, manager.GetComponentData<GhostRentComponent>(first).Value);
    }
}

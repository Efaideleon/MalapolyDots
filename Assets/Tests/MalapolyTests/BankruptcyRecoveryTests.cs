using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems;
using Assets.Scripts.DOTS.Mediator;
using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GamePlay;
using NUnit.Framework;
using Unity.Entities;
using Unity.NetCode;

public class BankruptcyRecoveryTests
{
    World world;
    EntityManager em;
    Entity debtor, creditor, game, property;
    SystemHandle bankruptcy;
    [SetUp] public void Setup()
    {
        world = new World("Bankruptcy recovery tests"); em = world.EntityManager;
        bankruptcy = world.CreateSystem<BankruptcySystem>();
        game = em.CreateSingleton(new GameStateComponent { State = GameState.Landing, AllPlacesInstantiated = true });
        debtor = Player(1, -100); creditor = Player(2, 200);
        property = em.CreateEntity(typeof(OwnerComponent), typeof(OwnerByEntityComponent), typeof(HouseCount), typeof(GhostRentComponent));
        em.SetComponentData(property, new OwnerComponent { ID = 1 });
        em.SetComponentData(property, new OwnerByEntityComponent { Entity = debtor });
        em.SetComponentData(property, new HouseCount { Value = 2 });
    }
    [TearDown] public void Cleanup() => world.Dispose();
    Entity Player(int id, int cash)
    {
        var player = em.CreateEntity(typeof(GhostOwner), typeof(GhostMoneyComponet), typeof(BankruptPlayer));
        em.SetComponentData(player, new GhostOwner { NetworkId = id });
        em.SetComponentData(player, new GhostMoneyComponet { Value = cash });
        var connection = em.CreateEntity(typeof(NetworkId));
        em.SetComponentData(connection, new NetworkId { Value = id });
        return player;
    }
    void Update() => bankruptcy.Update(world.Unmanaged);
    int Cash(Entity player) => em.GetComponentData<GhostMoneyComponet>(player).Value;
    [Test] public void DebtKeepsPlayerAndAssetsInTheMatch()
    {
        Update();
        Assert.IsFalse(em.GetComponentData<BankruptPlayer>(debtor).Value);
        Assert.AreEqual(1, em.GetComponentData<OwnerComponent>(property).ID);
        Assert.AreEqual(2, em.GetComponentData<HouseCount>(property).Value);
        Assert.AreEqual(GameState.Landing, em.GetComponentData<GameStateComponent>(game).State);
        Assert.IsTrue(BankruptcyRules.HasDebt(em));
    }
    [Test] public void RecoveringToExactlyZeroAllowsPlayToContinue()
    {
        Update(); em.SetComponentData(debtor, new GhostMoneyComponet { Value = 0 }); Update();
        Assert.IsFalse(em.GetComponentData<BankruptPlayer>(debtor).Value);
        Assert.IsFalse(BankruptcyRules.HasDebt(em));
    }
    [Test] public void RaisedMoneyPaysUnpaidRentOnceAndKeepsAnySurplus()
    {
        em.SetComponentData(debtor, new BankruptPlayer { UnpaidRent = 100, CreditorNetworkId = 2 });
        em.SetComponentData(debtor, new GhostMoneyComponet { Value = -60 }); Update();
        Assert.AreEqual(240, Cash(creditor));
        Assert.AreEqual(60, em.GetComponentData<BankruptPlayer>(debtor).UnpaidRent);
        Update(); Assert.AreEqual(240, Cash(creditor));
        em.SetComponentData(debtor, new GhostMoneyComponet { Value = 25 }); Update();
        Assert.AreEqual(300, Cash(creditor)); Assert.AreEqual(25, Cash(debtor));
        Assert.AreEqual(0, em.GetComponentData<BankruptPlayer>(debtor).UnpaidRent);
        Update(); Assert.AreEqual(300, Cash(creditor));
    }
    [Test] public void ExplicitDeclarationEliminatesPlayerAndReturnsAssets()
    {
        em.SetComponentData(debtor, new BankruptPlayer { Declared = true }); Update();
        Assert.IsTrue(em.GetComponentData<BankruptPlayer>(debtor).Value);
        Assert.AreEqual(PropertyConstants.Vacant, em.GetComponentData<OwnerComponent>(property).ID);
        Assert.AreEqual(0, em.GetComponentData<HouseCount>(property).Value);
        Assert.AreEqual(GameState.GameOver, em.GetComponentData<GameStateComponent>(game).State);
        Assert.AreEqual(2, em.GetComponentData<GameStateComponent>(game).WinnerNetworkId);
    }
    [Test] public void RentShortfallIsRecordedForRecovery()
    {
        em.SetComponentData(debtor, new GhostMoneyComponet { Value = 20 });
        em.AddComponentData(debtor, new LandingPaymentResolved());
        em.AddComponentData(debtor, new SpaceLandedOn { entity = property });
        em.CreateSingleton(new CurrentActivePlayer { Entity = debtor });
        em.AddComponent<DOTS.GameSpaces.PropertySpaceTag>(property);
        em.SetComponentData(property, new OwnerByEntityComponent { Entity = creditor });
        em.SetComponentData(property, new GhostRentComponent { Value = 100 });
        var connection = em.CreateEntity(typeof(NetworkId), typeof(NetworkStreamInGame));
        em.SetComponentData(connection, new NetworkId { Value = 1 });
        var rpc = em.CreateEntity(typeof(PayRentRpc), typeof(ReceiveRpcCommandRequest));
        em.SetComponentData(rpc, new ReceiveRpcCommandRequest { SourceConnection = connection });
        world.GetOrCreateSystem<PayRentSystem>().Update(world.Unmanaged);
        Assert.AreEqual(-80, Cash(debtor)); Assert.AreEqual(220, Cash(creditor));
        Assert.AreEqual(80, em.GetComponentData<BankruptPlayer>(debtor).UnpaidRent);
        Assert.AreEqual(2, em.GetComponentData<BankruptPlayer>(debtor).CreditorNetworkId);
    }
    [Test] public void RoundLimitWaitsForDebtResolution()
    {
        var state = em.GetComponentData<GameStateComponent>(game); state.RoundLimit = 8; em.SetComponentData(game, state);
        em.CreateSingleton(new CurrentRound { Value = 8 });
        var completion = world.GetOrCreateSystem<MatchCompletionSystem>();
        completion.Update(world.Unmanaged);
        Assert.AreNotEqual(GameState.GameOver, em.GetComponentData<GameStateComponent>(game).State);
        em.SetComponentData(debtor, new GhostMoneyComponet { Value = 0 }); completion.Update(world.Unmanaged);
        Assert.AreEqual(GameState.GameOver, em.GetComponentData<GameStateComponent>(game).State);
    }
    [Test] public void EndTurnWaitsForRecoveryThenAdvancesNormally()
    {
        var changeTurn = world.CreateSystem<ChangeTurnSystem>();
        em.CreateSingleton(new GeneralGhostStates());
        var sorted = em.CreateSingletonBuffer<PlayersSortedByNetId>();
        em.GetBuffer<PlayersSortedByNetId>(sorted).Add(new PlayersSortedByNetId { Name = "Debtor" });
        em.GetBuffer<PlayersSortedByNetId>(sorted).Add(new PlayersSortedByNetId { Name = "Creditor" });
        em.AddComponentData(debtor, new NameComponent { Value = "Debtor" });
        em.AddComponentData(creditor, new NameComponent { Value = "Creditor" });
        em.AddComponentData(debtor, new PlayerID { Value = 1 });
        em.AddComponentData(creditor, new PlayerID { Value = 2 });
        em.AddComponent<ActivePlayer>(debtor); em.AddComponent<ActivePlayer>(creditor);
        em.SetComponentEnabled<ActivePlayer>(creditor, false);
        var active = em.CreateSingleton(new CurrentActivePlayer { Entity = debtor });
        em.CreateSingleton(new CurrentPlayerID { Value = 1 });
        em.CreateSingleton(new CurrentPlayerComponent { entity = debtor });
        var connection = em.CreateEntity(typeof(NetworkId), typeof(NetworkStreamInGame));
        em.SetComponentData(connection, new NetworkId { Value = 1 });
        Entity Request()
        {
            var rpc = em.CreateEntity(typeof(ChangeTurnRpc), typeof(ReceiveRpcCommandRequest));
            em.SetComponentData(rpc, new ReceiveRpcCommandRequest { SourceConnection = connection });
            return rpc;
        }
        var rejected = Request(); changeTurn.Update(world.Unmanaged);
        Assert.IsFalse(em.Exists(rejected));
        Assert.AreEqual(debtor, em.GetComponentData<CurrentActivePlayer>(active).Entity);
        em.SetComponentData(debtor, new GhostMoneyComponet { Value = 0 });
        Request(); changeTurn.Update(world.Unmanaged);
        Assert.AreEqual(creditor, em.GetComponentData<CurrentActivePlayer>(active).Entity);
        Assert.AreEqual(GameState.Rolling, em.GetComponentData<GameStateComponent>(game).State);
    }
}

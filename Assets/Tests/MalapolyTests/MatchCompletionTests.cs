using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GamePlay;
using NUnit.Framework;
using Unity.Entities;
using Unity.NetCode;

public class MatchCompletionTests
{
    World world;
    EntityManager manager;
    Entity game, round;
    SimulationSystemGroup group;

    [SetUp] public void Setup()
    {
        world = new World("Match completion tests");
        manager = world.EntityManager;
        group = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
        group.AddSystemToUpdateList(world.CreateSystem<MatchCompletionSystem>());
        game = manager.CreateEntity(typeof(GameStateComponent));
        manager.SetComponentData(game, new GameStateComponent { AllPlacesInstantiated = true, RoundLimit = 8, State = GameState.Landing });
        round = manager.CreateEntity(typeof(CurrentRound));
    }
    [TearDown] public void Cleanup() => world.Dispose();
    Entity Player(int id, int money, bool bankrupt = false)
    {
        var entity = manager.CreateEntity(typeof(GhostMoneyComponet), typeof(BankruptPlayer), typeof(GhostOwner));
        manager.SetComponentData(entity, new GhostMoneyComponet { Value = money });
        manager.SetComponentData(entity, new BankruptPlayer { Value = bankrupt });
        manager.SetComponentData(entity, new GhostOwner { NetworkId = id });
        return entity;
    }
    void FinishRound(int count)
    {
        manager.SetComponentData(round, new CurrentRound { Value = count });
        group.Update();
    }
    [Test] public void MatchContinuesBeforeRoundLimit()
    {
        Player(1, 1000); Player(2, 900);
        FinishRound(7);
        Assert.AreNotEqual(GameState.GameOver, manager.GetComponentData<GameStateComponent>(game).State);
    }
    [Test] public void BuildingsAndPropertiesCountTowardWinner()
    {
        var owner = Player(1, 100); Player(2, 500);
        var property = manager.CreateEntity(typeof(OwnerByEntityComponent), typeof(GhostPriceComponent), typeof(HouseCount), typeof(HousePriceComponent));
        manager.SetComponentData(property, new OwnerByEntityComponent { Entity = owner });
        manager.SetComponentData(property, new GhostPriceComponent { Value = 200 });
        manager.SetComponentData(property, new HouseCount { Value = 5 });
        manager.SetComponentData(property, new HousePriceComponent { Value = 50 });
        FinishRound(8);
        var result = manager.GetComponentData<GameStateComponent>(game);
        Assert.AreEqual(GameState.GameOver, result.State);
        Assert.AreEqual(1, result.WinnerNetworkId);
        Assert.AreEqual(550, manager.GetComponentData<BankruptPlayer>(owner).FinalNetWorth);
    }
    [Test] public void EqualNetWorthIsDraw()
    {
        Player(1, 500); Player(2, 500);
        FinishRound(8);
        Assert.AreEqual(0, manager.GetComponentData<GameStateComponent>(game).WinnerNetworkId);
    }
    [Test] public void BankruptPlayerCannotWin()
    {
        Player(1, 1000, true); Player(2, 100);
        FinishRound(8);
        Assert.AreEqual(2, manager.GetComponentData<GameStateComponent>(game).WinnerNetworkId);
    }
    [Test] public void ResultsRemainFrozenAfterMatchEnds()
    {
        var player = Player(1, 500); Player(2, 100);
        FinishRound(8);
        manager.SetComponentData(player, new GhostMoneyComponet { Value = 1000 });
        group.Update();
        Assert.AreEqual(500, manager.GetComponentData<BankruptPlayer>(player).FinalNetWorth);
    }
    [Test] public void DisconnectedPlayerIsEliminatedAndRemainingPlayerWins()
    {
        group.AddSystemToUpdateList(world.CreateSystem<BankruptcySystem>());
        group.SortSystems();
        var disconnected = Player(1, 500);
        Player(2, 100);
        var connection = manager.CreateEntity(typeof(NetworkId));
        manager.SetComponentData(connection, new NetworkId { Value = 2 });
        group.Update();
        Assert.IsTrue(manager.GetComponentData<BankruptPlayer>(disconnected).Value);
        Assert.AreEqual(2, manager.GetComponentData<GameStateComponent>(game).WinnerNetworkId);
    }
    [TestCase(200, 50, 5, 450)]
    [TestCase(200, 50, 0, 200)]
    public void BuildingValuesIncludeHotel(int price, int house, int count, int value)
        => Assert.AreEqual(value, MatchRules.PropertyValue(price, house, count));
}

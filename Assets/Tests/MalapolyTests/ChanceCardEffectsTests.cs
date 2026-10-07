using System.Collections.Generic;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters.CharacterSpawner;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class ChanceCardEffectsTests
{
    World world;
    EntityManager em;
    Entity player, game, chance;
    BlobAssetReference<WaypointsBlobAsset> route;
    NativeParallelHashMap<int, Entity> board;

    [SetUp] public void Setup()
    {
        world = new World("Chance card effects");
        em = world.EntityManager;
        game = em.CreateSingleton(new GameStateComponent { State = GameState.Landing });
        chance = em.CreateEntity(typeof(ChanceSpaceTag));
        em.AddBuffer<ChanceActionDataBuffer>(chance);
        player = Player(1000);
        em.AddComponent<ActivePlayer>(player);
        em.AddComponent<GhostChanceCardPicked>(player);
        em.AddComponent<LandingPaymentResolved>(player);
        em.AddComponentData(player, new SpaceLandedOn { entity = chance });
        em.AddComponentData(player, LocalTransform.FromPosition(new float3(72, 0, 0)));
        em.AddComponentData(player, new PlayerBoardIndex { Value = 36 });
        em.AddComponentData(player, new PlayerWaypointIndex { Value = 72 });
        em.AddComponent<TargetPosition>(player);
        em.AddComponent<RemainingMoves>(player);
        em.AddComponent<PlayerMovementState>(player);
        em.AddComponent<FinalArrived>(player);
        em.AddComponent<ReachedTargetPosition>(player);
        em.AddComponent<JailState>(player);
        em.CreateSingleton(new CurrentActivePlayer { Entity = player });
    }

    [TearDown] public void Cleanup()
    {
        world.Dispose();
        if (route.IsCreated) route.Dispose();
        if (board.IsCreated) board.Dispose();
    }

    Entity Player(int money)
    {
        var entity = em.CreateEntity(typeof(GhostMoneyComponet), typeof(BankruptPlayer));
        em.SetComponentData(entity, new GhostMoneyComponet { Value = money });
        return entity;
    }

    Entity Deed(Entity owner, int buildings)
    {
        var deed = em.CreateEntity(typeof(OwnerByEntityComponent), typeof(HouseCount));
        em.SetComponentData(deed, new OwnerByEntityComponent { Entity = owner });
        em.SetComponentData(deed, new HouseCount { Value = buildings });
        return deed;
    }

    int Apply(ChanceEffect effect, int amount = 0, int hotel = 0, int destination = 0)
    {
        using var feedback = new EntityCommandBuffer(Allocator.Temp);
        int delta = ChanceCardRules.Apply(em, feedback, player, new ChanceActionDataBuffer
            { effect = effect, amount = amount, hotelAmount = hotel, targetBoardIndex = destination });
        feedback.Playback(em);
        return delta;
    }

    int Cash(Entity entity) => em.GetComponentData<GhostMoneyComponet>(entity).Value;

    [Test] public void RepairsCountOnlyOwnedBuildingsAndHotelsReplaceHouses()
    {
        Deed(player, 2); Deed(player, 5); Deed(player, 0); Deed(Player(1000), 4);
        Assert.AreEqual(-150, Apply(ChanceEffect.BuildingRepairs, 25, 100));
        Assert.AreEqual(850, Cash(player));
    }

    [Test] public void EmptyPortfolioHasNoRepairBillOrPropertyIncome()
    {
        Assert.AreEqual(0, Apply(ChanceEffect.BuildingRepairs, 25, 100));
        Assert.AreEqual(0, Apply(ChanceEffect.PropertyIncome, 25));
        Assert.AreEqual(1000, Cash(player));
    }

    [Test] public void PropertyIncomeCountsDeedsInsteadOfBuildings()
    {
        Deed(player, 4); Deed(player, 5); Deed(Player(1000), 2);
        Assert.AreEqual(50, Apply(ChanceEffect.PropertyIncome, 25));
    }

    [TestCase(ChanceEffect.PayEachPlayer, -100)]
    [TestCase(ChanceEffect.CollectFromEachPlayer, 100)]
    public void PlayerTransfersConserveMoneyAndSkipEliminatedPlayers(ChanceEffect effect, int expected)
    {
        var first = Player(100); var second = Player(100); var eliminated = Player(100); var declared = Player(100);
        em.SetComponentData(eliminated, new BankruptPlayer { Value = true });
        em.SetComponentData(declared, new BankruptPlayer { Declared = true });
        Assert.AreEqual(expected, Apply(effect, 50));
        Assert.AreEqual(1200, Cash(player) + Cash(first) + Cash(second));
        Assert.AreEqual(100, Cash(eliminated)); Assert.AreEqual(100, Cash(declared));
    }

    [Test] public void ChargedOpponentCanRecoverDebtInsteadOfBeingEliminated()
    {
        var other = Player(10);
        Assert.AreEqual(25, Apply(ChanceEffect.CollectFromEachPlayer, 25));
        Assert.AreEqual(-15, Cash(other));
        Assert.IsFalse(em.GetComponentData<BankruptPlayer>(other).Value);
        Assert.IsTrue(BankruptcyRules.HasDebt(em));
    }

    void BoardAndRoute()
    {
        using (var builder = new BlobBuilder(Allocator.Temp))
        {
            ref var root = ref builder.ConstructRoot<WaypointsBlobAsset>();
            var points = builder.Allocate(ref root.Waypoints, 80);
            for (int i = 0; i < points.Length; i++)
                points[i] = new Waypoint { Position = new float3(i, 0, 0), IsLandingSpot = i % 2 == 0 };
            route = builder.CreateBlobAssetReference<WaypointsBlobAsset>(Allocator.Persistent);
        }
        em.CreateSingleton(new WaypointsBlobRef { Reference = route });
        board = new NativeParallelHashMap<int, Entity>(40, Allocator.Persistent);
        for (int i = 0; i < 40; i++)
        {
            var tile = em.CreateEntity(typeof(BoardIndexComponent));
            em.SetComponentData(tile, new BoardIndexComponent { Value = i });
            board.Add(i, tile);
        }
        em.AddComponent<JailSpaceTag>(board[10]);
        em.CreateSingleton(new IndexToBoardHashMap { Map = board });
        em.CreateSingletonBuffer<PlayerArrivedAtDestinationEvent>();
        em.CreateEntity(typeof(CharactersSpawnedTag), typeof(RollAmountComponent));
    }

    [TestCase(0, 4, 1200)]
    [TestCase(39, 3, 1000)]
    public void TravelWalksToDestinationAndPaysGoExactlyOnce(int destination, int moves, int finalCash)
    {
        BoardAndRoute();
        em.SetComponentData(player, new LandingPaymentResolved { Value = true });
        Apply(ChanceEffect.AdvanceToSpace, destination: destination);
        Assert.AreEqual(moves, em.GetComponentData<RemainingMoves>(player).Value);
        Assert.AreEqual(GameState.Walking, em.GetComponentData<GameStateComponent>(game).State);
        Assert.IsFalse(em.GetComponentData<LandingPaymentResolved>(player).Value);
        var waypoint = world.CreateSystem<CharacterWaypointSystem>();
        var detector = world.CreateSystem<SpaceDetectorSystem>();
        var gameplay = world.CreateSystem<GamePlaySystem>();
        for (int i = 0; i < moves * 2; i++)
        {
            int next = (em.GetComponentData<PlayerWaypointIndex>(player).Value + 1) % 80;
            em.SetComponentData(player, LocalTransform.FromPosition(new float3(next, 0, 0)));
            waypoint.Update(world.Unmanaged); detector.Update(world.Unmanaged); gameplay.Update(world.Unmanaged);
        }
        Assert.AreEqual(finalCash, Cash(player));
        Assert.AreEqual(destination, em.GetComponentData<PlayerBoardIndex>(player).Value);
        Assert.AreEqual(board[destination], em.GetComponentData<SpaceLandedOn>(player).entity);
        Assert.AreEqual(GameState.Landing, em.GetComponentData<GameStateComponent>(game).State);
        Assert.AreEqual(1u, em.GetComponentData<GameStateComponent>(game).LandingSequence);
        Assert.IsFalse(em.GetComponentData<LandingPaymentResolved>(player).Value);
    }

    [Test] public void NearestTransportWrapsAroundTheBoard()
    {
        BoardAndRoute();
        em.AddComponentData(board[5], new PropertyRentKindComponent { Value = PropertyRentKind.Transport });
        em.AddComponentData(board[15], new PropertyRentKindComponent { Value = PropertyRentKind.Transport });
        Apply(ChanceEffect.AdvanceToTransport);
        Assert.AreEqual(9, em.GetComponentData<RemainingMoves>(player).Value);
    }

    [Test] public void JailTravelSkipsCornersAndNeverAwardsGoMoney()
    {
        BoardAndRoute();
        Apply(ChanceEffect.GoToJail);
        Assert.AreEqual(1000, Cash(player));
        Assert.AreEqual(10, em.GetComponentData<PlayerBoardIndex>(player).Value);
        Assert.AreEqual(20, em.GetComponentData<PlayerWaypointIndex>(player).Value);
        Assert.AreEqual(new float3(20, 0, 0), em.GetComponentData<LocalTransform>(player).Position);
        Assert.AreEqual(board[10], em.GetComponentData<SpaceLandedOn>(player).entity);
        Assert.IsTrue(em.GetComponentData<JailState>(player).InJail);
        Assert.AreEqual(2, em.GetComponentData<JailState>(player).TurnsInJail);
        Assert.AreEqual(MoveState.Idle, em.GetComponentData<PlayerMovementState>(player).Value);
    }

    [Test] public void FreePassIsKeptUntilJailedAndCanOnlyBeUsedOnce()
    {
        Apply(ChanceEffect.JailFreeCard);
        Assert.IsFalse(ChanceCardRules.UseJailFreeCard(em, player));
        var jail = em.GetComponentData<JailState>(player); jail.InJail = true; jail.TurnsInJail = 1;
        em.SetComponentData(player, jail);
        Assert.IsTrue(ChanceCardRules.UseJailFreeCard(em, player));
        Assert.IsFalse(em.GetComponentData<JailState>(player).InJail);
        Assert.IsFalse(em.GetComponentData<JailState>(player).HasGetOutOfJailFreeCard);
        Assert.IsFalse(ChanceCardRules.UseJailFreeCard(em, player));
    }

    [Test] public void DeckIsSharedAcrossChanceTilesAndReshufflesOnlyAfterEveryCard()
    {
        var system = world.CreateSystem<PickRandomChanceCardSystem>();
        em.CreateSingleton(new RandomValueComponent { Value = new Unity.Mathematics.Random(456) });
        var otherTile = em.CreateEntity(typeof(ChanceSpaceTag)); em.AddBuffer<ChanceActionDataBuffer>(otherTile);
        for (int i = 0; i < 22; i++)
        {
            var card = new ChanceActionDataBuffer { id = i, msg = "Reward", amount = 1 };
            em.GetBuffer<ChanceActionDataBuffer>(chance).Add(card); em.GetBuffer<ChanceActionDataBuffer>(otherTile).Add(card);
        }
        var seen = new HashSet<int>();
        for (int i = 0; i < 22; i++)
        {
            em.SetComponentData(player, new SpaceLandedOn { entity = i % 2 == 0 ? chance : otherTile });
            em.SetComponentData(player, new LandingPaymentResolved());
            system.Update(world.Unmanaged); system.Update(world.Unmanaged);
            Assert.IsTrue(seen.Add(em.GetComponentData<GhostChanceCardPicked>(player).id));
        }
        Assert.AreEqual(1022, Cash(player));
        em.SetComponentData(player, new LandingPaymentResolved()); system.Update(world.Unmanaged);
        Assert.AreEqual(23u, em.GetComponentData<GhostChanceCardPicked>(player).DrawSequence);
    }
}

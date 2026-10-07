using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters.CharacterSpawner;
using DOTS.GamePlay;
using NUnit.Framework;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

public class CharacterSpaceArrangementTests
{
    World world;
    EntityManager manager;
    SimulationSystemGroup group;
    Entity game, active;
    BlobAssetReference<WaypointsBlobAsset> route;
    readonly float3 center = new float3(20, 0.5f, 30);

    [SetUp]
    public void Setup()
    {
        world = new World("Shared square arrangement tests");
        manager = world.EntityManager;
        group = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
        group.AddSystemToUpdateList(world.CreateSystem<CharacterSpaceArrangementSystem>());
        game = manager.CreateEntity(typeof(GameStateComponent));
        manager.SetComponentData(game, new GameStateComponent { State = GameState.Rolling });
        active = manager.CreateEntity(typeof(CurrentActivePlayer));
        using var builder = new BlobBuilder(Allocator.Temp);
        ref var root = ref builder.ConstructRoot<WaypointsBlobAsset>();
        var points = builder.Allocate(ref root.Waypoints, 3);
        points[0] = new Waypoint { Position = center, IsLandingSpot = true };
        points[1] = new Waypoint { Position = center + new float3(10, 0, 0), IsLandingSpot = false };
        points[2] = new Waypoint { Position = center + new float3(20, 0, 0), IsLandingSpot = true };
        route = builder.CreateBlobAssetReference<WaypointsBlobAsset>(Allocator.Persistent);
        manager.SetComponentData(manager.CreateEntity(typeof(WaypointsBlobRef)), new WaypointsBlobRef { Reference = route });
    }

    [TearDown]
    public void Cleanup()
    {
        world.Dispose();
        route.Dispose();
    }

    Entity Player(int id, int board = 0, int waypoint = 0)
    {
        var player = manager.CreateEntity(typeof(CharacterFlag), typeof(GhostOwner), typeof(PlayerBoardIndex),
            typeof(PlayerWaypointIndex), typeof(PlayerMovementState), typeof(RemainingMoves), typeof(LocalTransform), typeof(TargetPosition));
        manager.SetComponentData(player, new GhostOwner { NetworkId = id });
        manager.SetComponentData(player, new PlayerBoardIndex { Value = board });
        manager.SetComponentData(player, new PlayerWaypointIndex { Value = waypoint });
        manager.SetComponentData(player, new PlayerMovementState { Value = MoveState.Idle });
        manager.SetComponentData(player, LocalTransform.FromPosition(route.Value.Waypoints[waypoint].Position));
        manager.SetComponentData(player, new TargetPosition { Value = new float3(100, 0, 100) });
        return player;
    }

    void Tick(float seconds = 1f)
    {
        world.SetTime(new TimeData(world.Time.ElapsedTime + seconds, seconds));
        group.Update();
    }

    float3 Position(Entity player) => manager.GetComponentData<LocalTransform>(player).Position;

    [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
    public void PiecesSharingSquareHaveClearanceAndRemainCentered(int count)
    {
        var players = new Entity[count];
        for (int i = 0; i < count; i++) players[i] = Player(i + 1);
        Tick();
        float3 average = float3.zero;
        for (int i = 0; i < count; i++)
        {
            average += Position(players[i]);
            Assert.AreEqual(center.y, Position(players[i]).y);
            for (int j = i + 1; j < count; j++)
                Assert.GreaterOrEqual(math.distance(Position(players[i]), Position(players[j])), CharacterSpaceLayout.PieceSpacing - 0.001f);
        }
        Assert.Less(math.distance(center, average / count), 0.001f);
    }

    [Test]
    public void SlotsUsePlayerIdentityRegardlessOfCreationOrder()
    {
        var third = Player(3); var first = Player(1); var second = Player(2);
        Tick();
        var expected = new[] { Position(first), Position(second), Position(third) };
        manager.DestroyEntity(first); manager.DestroyEntity(second); manager.DestroyEntity(third);
        first = Player(1); second = Player(2); third = Player(3);
        Tick();
        Assert.AreEqual(expected[0], Position(first));
        Assert.AreEqual(expected[1], Position(second));
        Assert.AreEqual(expected[2], Position(third));
    }

    [Test]
    public void WalkingPieceKeepsItsPositionAndRemainingPieceRecenters()
    {
        var first = Player(1); var departing = Player(2);
        Tick();
        manager.SetComponentData(departing, new PlayerMovementState { Value = MoveState.Walking });
        var walkingPosition = Position(departing);
        Tick();
        Assert.AreEqual(walkingPosition, Position(departing));
        Assert.AreEqual(center, Position(first));
    }

    [Test]
    public void RollStartingBeforeMovementUpdateDoesNotPullPlayerBack()
    {
        var resting = Player(1); var departing = Player(2);
        Tick();
        manager.SetComponentData(game, new GameStateComponent { State = GameState.Walking });
        manager.SetComponentData(active, new CurrentActivePlayer { Entity = departing });
        var position = Position(departing);
        Tick();
        Assert.AreEqual(position, Position(departing));
        Assert.AreEqual(center, Position(resting));
    }

    [Test]
    public void DifferentSquaresAndIntermediateWaypointsDoNotShareSlots()
    {
        var first = Player(1);
        var otherSquare = Player(2, 1, 2);
        var passing = Player(3, 0, 1);
        Tick();
        Assert.AreEqual(center, Position(first));
        Assert.AreEqual(route.Value.Waypoints[2].Position, Position(otherSquare));
        Assert.AreEqual(route.Value.Waypoints[1].Position, Position(passing));
    }

    [Test]
    public void TeleportIntoOccupiedSquareUsesDestinationCenterWithoutChangingRoute()
    {
        var waiting = Player(1, 1, 2); var arriving = Player(2);
        manager.SetComponentData(arriving, new PlayerBoardIndex { Value = 1 });
        manager.SetComponentData(arriving, new PlayerWaypointIndex { Value = 2 });
        manager.SetComponentData(arriving, LocalTransform.FromPosition(route.Value.Waypoints[2].Position));
        var target = manager.GetComponentData<TargetPosition>(arriving).Value;
        Tick();
        Assert.GreaterOrEqual(math.distance(Position(waiting), Position(arriving)), CharacterSpaceLayout.PieceSpacing - 0.001f);
        Assert.AreEqual(target, manager.GetComponentData<TargetPosition>(arriving).Value);
        Assert.AreEqual(1, manager.GetComponentData<PlayerBoardIndex>(arriving).Value);
        Assert.AreEqual(2, manager.GetComponentData<PlayerWaypointIndex>(arriving).Value);
    }

    [Test]
    public void ArrangementSlidesAndDoesNotDriftAcrossUpdates()
    {
        var first = Player(1); var second = Player(2);
        Tick(1f / 60f);
        Assert.Greater(math.distance(center, Position(first)), 0f);
        Assert.Less(math.distance(Position(first), Position(second)), CharacterSpaceLayout.PieceSpacing);
        Tick();
        var settled = Position(first);
        Tick();
        Assert.AreEqual(settled, Position(first));
    }
}

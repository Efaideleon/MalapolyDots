using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.Mediator;
using DOTS.Characters.CharacterSpawner;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

public class JailSystemTest
{
    private World world;
    private EntityManager manager;
    private Entity player, jail, connection;
    private BlobAssetReference<WaypointsBlobAsset> route;

    [SetUp]
    public void SetUp()
    {
        world = new World("Jail test");
        manager = world.EntityManager;
        var system = world.CreateSystem<JailSystem>();
        world.GetOrCreateSystemManaged<SimulationSystemGroup>().AddSystemToUpdateList(system);
        connection = manager.CreateEntity(typeof(NetworkId), typeof(NetworkStreamInGame));
        manager.SetComponentData(connection, new NetworkId { Value = 1 });
        jail = manager.CreateEntity(typeof(JailSpaceTag), typeof(BoardIndexComponent));
        manager.SetComponentData(jail, new BoardIndexComponent { Value = 10 });
        var goToJail = manager.CreateEntity(typeof(GoToJailTag));
        player = manager.CreateEntity(typeof(GhostOwner), typeof(SpaceLandedOn), typeof(LocalTransform),
            typeof(PlayerBoardIndex), typeof(PlayerWaypointIndex), typeof(TargetPosition),
            typeof(RemainingMoves), typeof(PlayerMovementState), typeof(FinalArrived),
            typeof(ReachedTargetPosition), typeof(JailState));
        manager.SetComponentData(player, new GhostOwner { NetworkId = 1 });
        manager.SetComponentData(player, new SpaceLandedOn { entity = goToJail });
        manager.SetComponentData(player, LocalTransform.FromPosition(new float3(60, 0, 0)));
        manager.SetComponentData(player, new PlayerBoardIndex { Value = 30 });
        manager.SetComponentData(player, new PlayerWaypointIndex { Value = 60 });
        manager.SetComponentData(manager.CreateEntity(typeof(CurrentActivePlayer)), new CurrentActivePlayer { Entity = player });
        manager.SetComponentData(manager.CreateEntity(typeof(GameStateComponent)), new GameStateComponent { State = GameState.Landing });
        using (var builder = new BlobBuilder(Allocator.Temp))
        {
            ref var root = ref builder.ConstructRoot<WaypointsBlobAsset>();
            var points = builder.Allocate(ref root.Waypoints, 80);
            for (int i = 0; i < points.Length; i++)
                points[i] = new Waypoint { Position = new float3(i, 0, 0), IsLandingSpot = i % 2 == 0 };
            route = builder.CreateBlobAssetReference<WaypointsBlobAsset>(Allocator.Persistent);
        }
        manager.SetComponentData(manager.CreateEntity(typeof(WaypointsBlobRef)), new WaypointsBlobRef { Reference = route });
    }

    [TearDown]
    public void TearDown()
    {
        world.Dispose();
        route.Dispose();
    }

    private void Confirm()
    {
        var rpc = manager.CreateEntity(typeof(GoToJailRpc), typeof(ReceiveRpcCommandRequest));
        manager.SetComponentData(rpc, new ReceiveRpcCommandRequest { SourceConnection = connection });
    }

    private int TurnRequests()
    {
        using var query = manager.CreateEntityQuery(typeof(ChangeTurnRpc), typeof(ReceiveRpcCommandRequest));
        return query.CalculateEntityCount();
    }

    [Test]
    public void ConfirmationMovesPlayerAndQueuesOneTurnChange()
    {
        Confirm();
        Confirm(); // A repeated click must not transfer or end the turn twice.
        world.Update();
        Assert.AreEqual(new float3(20, 0, 0), manager.GetComponentData<LocalTransform>(player).Position);
        Assert.AreEqual(10, manager.GetComponentData<PlayerBoardIndex>(player).Value);
        Assert.AreEqual(20, manager.GetComponentData<PlayerWaypointIndex>(player).Value);
        Assert.AreEqual(jail, manager.GetComponentData<SpaceLandedOn>(player).entity);
        Assert.IsTrue(manager.GetComponentData<JailState>(player).InJail);
        Assert.AreEqual(2, manager.GetComponentData<JailState>(player).TurnsInJail);
        Assert.AreEqual(MoveState.Idle, manager.GetComponentData<PlayerMovementState>(player).Value);
        Assert.AreEqual(0, manager.GetComponentData<RemainingMoves>(player).Value);
        Assert.IsFalse(manager.GetComponentData<FinalArrived>(player).Value);
        Assert.AreEqual(1, TurnRequests());
    }

    [Test]
    public void LandingWaitsForConfirmation()
    {
        world.Update();
        Assert.AreEqual(30, manager.GetComponentData<PlayerBoardIndex>(player).Value);
        Assert.IsFalse(manager.GetComponentData<JailState>(player).InJail);
        Assert.AreEqual(0, TurnRequests());
    }

    [Test]
    public void VisitingJailDoesNotImprisonPlayer()
    {
        manager.SetComponentData(player, new SpaceLandedOn { entity = jail });
        Confirm();
        world.Update();
        Assert.IsFalse(manager.GetComponentData<JailState>(player).InJail);
        Assert.AreEqual(0, TurnRequests());
    }

    [Test]
    public void AnotherPlayersConfirmationIsIgnored()
    {
        manager.SetComponentData(connection, new NetworkId { Value = 2 });
        Confirm();
        world.Update();
        Assert.AreEqual(30, manager.GetComponentData<PlayerBoardIndex>(player).Value);
        Assert.IsFalse(manager.GetComponentData<JailState>(player).InJail);
        Assert.AreEqual(0, TurnRequests());
    }
}

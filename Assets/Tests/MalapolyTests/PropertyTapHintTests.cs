using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;

public class PropertyTapHintTests
{
    World world;
    EntityManager manager;
    Entity player, property;
    GameStateComponent game;

    [SetUp]
    public void Setup()
    {
        world = new World("Property tap hint tests");
        manager = world.EntityManager;
        property = manager.CreateEntity(typeof(PropertySpaceTag));
        player = manager.CreateEntity(typeof(GhostOwner), typeof(PlayerMovementState), typeof(SpaceLandedOn), typeof(BankruptPlayer));
        manager.SetComponentData(player, new GhostOwner { NetworkId = 7 });
        manager.SetComponentData(player, new PlayerMovementState { Value = MoveState.Idle });
        manager.SetComponentData(player, new SpaceLandedOn { entity = property });
        game = new GameStateComponent { State = GameState.Landing, AllPlacesInstantiated = true };
    }

    [TearDown]
    public void Cleanup() => world.Dispose();

    [TestCase(GameState.Landing, true)]
    [TestCase(GameState.Rolling, true)]
    [TestCase(GameState.Walking, false)]
    [TestCase(GameState.GameOver, false)]
    public void CueMatchesTheLocalPlayersStationaryActions(GameState state, bool expected)
    {
        game.State = state;
        Assert.AreEqual(expected ? property : Entity.Null, PropertyTapHintRules.EligibleProperty(manager, game, player, 7, false));
    }

    [Test]
    public void OtherPlayersAndSpecialSpacesHaveNoTapPrompt()
    {
        Assert.AreEqual(Entity.Null, PropertyTapHintRules.EligibleProperty(manager, game, player, 8, false));
        manager.RemoveComponent<PropertySpaceTag>(property);
        Assert.AreEqual(Entity.Null, PropertyTapHintRules.EligibleProperty(manager, game, player, 7, false));
    }

    [Test]
    public void MovingUnloadedIndebtedAndBankruptPlayersHaveNoCue()
    {
        Assert.AreEqual(Entity.Null, PropertyTapHintRules.EligibleProperty(manager, game, player, 7, true));
        game.AllPlacesInstantiated = false;
        Assert.AreEqual(Entity.Null, PropertyTapHintRules.EligibleProperty(manager, game, player, 7, false));
        game.AllPlacesInstantiated = true;
        manager.SetComponentData(player, new PlayerMovementState { Value = MoveState.Walking });
        Assert.AreEqual(Entity.Null, PropertyTapHintRules.EligibleProperty(manager, game, player, 7, false));
        manager.SetComponentData(player, new PlayerMovementState { Value = MoveState.Idle });
        manager.SetComponentData(player, new BankruptPlayer { Value = true });
        Assert.AreEqual(Entity.Null, PropertyTapHintRules.EligibleProperty(manager, game, player, 7, false));
    }

    [Test]
    public void MissingAndDestroyedEntitiesDoNotProduceACue()
    {
        Assert.AreEqual(Entity.Null, PropertyTapHintRules.EligibleProperty(manager, game, Entity.Null, 7, false));
        manager.DestroyEntity(property);
        Assert.AreEqual(Entity.Null, PropertyTapHintRules.EligibleProperty(manager, game, player, 7, false));
    }

    [Test]
    public void OnlyAValidPropertyTapDismissesTheTutorialForTheMatch()
    {
        var hint = new PropertyTapHintState { Property = property };
        hint.ObserveTap(new LocalPropertyTapResult { Sequence = 1, Property = property, BlockedByUI = true });
        Assert.IsFalse(hint.Learned);
        hint.ObserveTap(new LocalPropertyTapResult { Sequence = 2, Property = manager.CreateEntity() });
        Assert.IsFalse(hint.Learned);
        hint.ObserveTap(new LocalPropertyTapResult { Sequence = 3 });
        Assert.IsFalse(hint.Learned);
        hint.ObserveTap(new LocalPropertyTapResult { Sequence = 4, Property = property });
        Assert.IsTrue(hint.Learned);
        hint.Property = manager.CreateEntity();
        hint.ObserveTap(new LocalPropertyTapResult { Sequence = 5 });
        Assert.IsTrue(hint.Learned);
    }

    [Test]
    public void EarlierTapCannotDismissANewlyAvailableCue()
    {
        var hint = new PropertyTapHintState();
        var tap = new LocalPropertyTapResult { Sequence = 1, Property = property };
        hint.ObserveTap(tap);
        hint.Property = property;
        hint.ObserveTap(tap);
        Assert.IsFalse(hint.Learned);
        tap.Sequence++;
        hint.ObserveTap(tap);
        Assert.IsTrue(hint.Learned);
    }

    [Test]
    public void HoverUsesActualGeometryAndRejectsPropertiesBehindAnotherOne()
    {
        using var collider = Unity.Physics.BoxCollider.Create(new BoxGeometry {
            Center = float3.zero, Size = new float3(2), Orientation = quaternion.identity, BevelRadius = 0
        }, CollisionFilter.Default);
        manager.AddComponentData(property, new PhysicsCollider { Value = collider });
        manager.AddComponentData(property, new LocalToWorld { Value = float4x4.identity });
        var behind = manager.CreateEntity(typeof(PropertySpaceTag), typeof(PhysicsCollider), typeof(LocalToWorld));
        manager.SetComponentData(behind, new PhysicsCollider { Value = collider });
        manager.SetComponentData(behind, new LocalToWorld { Value = float4x4.Translate(new float3(0, 0, 5)) });
        using var query = PropertyTapHintRules.CreateCollidersQuery(manager);
        Assert.IsTrue(PropertyTapHintRules.IsHovered(manager, query, property, new float3(0, 0, -10), new float3(0, 0, 10)));
        Assert.IsFalse(PropertyTapHintRules.IsHovered(manager, query, behind, new float3(0, 0, -10), new float3(0, 0, 10)));
        Assert.IsFalse(PropertyTapHintRules.IsHovered(manager, query, property, new float3(8, 0, -10), new float3(8, 0, 10)));
    }
}

using Assets.Scripts.DOTS.Characters;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using Input;
using NUnit.Framework;
using Unity.Entities;
using Unity.NetCode;

public class PropertyTapTests
{
    World world;
    EntityManager em;
    PresentationSystemGroup group;
    Entity player, result, request, property;

    [SetUp]
    public void SetUp()
    {
        world = new World("Local property tap regression");
        em = world.EntityManager;
        group = world.GetOrCreateSystemManaged<PresentationSystemGroup>();
        group.AddSystemToUpdateList(world.CreateSystem<RayCastSystem>());
        group.AddSystemToUpdateList(world.CreateSystem<PropertyClickSystem>());
        group.SortSystems();
        request = em.CreateEntity(typeof(LocalPropertyTap));
        using var query = em.CreateEntityQuery(typeof(LocalPropertyTapResult));
        result = query.GetSingletonEntity();
        property = em.CreateEntity(typeof(PropertySpaceTag));
        player = em.CreateEntity(typeof(ClickedPropertyComponent), typeof(GhostOwnerIsLocal));
    }

    [Test]
    public void TapIsProcessedWithoutNetworkTimeOrPredictedCharacters()
    {
        em.SetComponentData(request, new LocalPropertyTap { Sequence = 1, RayEnd = new Unity.Mathematics.float3(0, 0, 10) });
        group.Update();
        Assert.AreEqual(1u, em.GetComponentData<LocalPropertyTapResult>(result).Sequence);
        Assert.AreEqual(Entity.Null, em.GetComponentData<LocalPropertyTapResult>(result).Property);
    }

    [Test]
    public void RepeatedPropertyTapSelectsWithoutSimulateOrActivePlayerTags()
    {
        for (uint sequence = 1; sequence <= 2; sequence++)
        {
            em.SetComponentData(player, new ClickedPropertyComponent());
            em.SetComponentData(result, new LocalPropertyTapResult { Sequence = sequence, Property = property });
            group.Update();
            Assert.AreEqual(property, em.GetComponentData<ClickedPropertyComponent>(player).entity);
        }
    }

    [Test]
    public void MissClearsSelection()
    {
        em.SetComponentData(player, new ClickedPropertyComponent { entity = property });
        em.SetComponentData(result, new LocalPropertyTapResult { Sequence = 1 });
        group.Update();
        Assert.AreEqual(Entity.Null, em.GetComponentData<ClickedPropertyComponent>(player).entity);
    }

    [Test]
    public void UiTapDoesNotRaycastOrChangePropertySelection()
    {
        em.SetComponentData(player, new ClickedPropertyComponent { entity = property });
        em.SetComponentData(request, new LocalPropertyTap { Sequence = 1, BlockedByUI = true });
        group.Update();
        Assert.IsTrue(em.GetComponentData<LocalPropertyTapResult>(result).BlockedByUI);
        Assert.AreEqual(property, em.GetComponentData<ClickedPropertyComponent>(player).entity);
    }

    [TestCase(GameState.Walking, MoveState.Idle, false)]
    [TestCase(GameState.Landing, MoveState.Walking, false)]
    [TestCase(GameState.GameOver, MoveState.Idle, false)]
    [TestCase(GameState.Landing, MoveState.Idle, true)]
    [TestCase(GameState.Rolling, MoveState.Idle, true)]
    public void ActionsRequireStationaryPlayer(GameState gameState, MoveState movement, bool expected)
    {
        Assert.AreEqual(expected, PropertyActionRules.CanOpen(gameState, movement, property, property));
    }

    [Test]
    public void ActionsRejectOtherPropertiesAndEmptySpace()
    {
        Assert.IsFalse(PropertyActionRules.CanOpen(GameState.Landing, MoveState.Idle, property, em.CreateEntity()));
        Assert.IsFalse(PropertyActionRules.CanOpen(GameState.Landing, MoveState.Idle, property, Entity.Null));
    }

    [TearDown]
    public void TearDown() => world.Dispose();
}

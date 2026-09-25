using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters.CharacterSpawner;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.Mediator.Systems;
using NUnit.Framework;
using Unity.Entities;
using Unity.NetCode;

public class LandingPopupTests
{
    World world;
    EntityManager em;
    Entity game, active, first, second, property;
    SystemHandle popup;

    [SetUp]
    public void Setup()
    {
        world = new World("Landing popup regression");
        em = world.EntityManager;
        popup = world.GetOrCreateSystem<SpaceActionsPanelPopupSystem>();
        property = em.CreateEntity(typeof(PropertySpaceTag));
        first = Player();
        second = Player();
        em.SetComponentEnabled<ActivePlayer>(second, false);
        active = em.CreateSingleton(new CurrentActivePlayer { Entity = first });
        game = em.CreateSingleton(new GameStateComponent
        {
            State = GameState.Landing, LandingSequence = 1,
            LandingPlayer = first, LandingSpace = property
        });
        em.CreateEntity(typeof(GhostDataLoadedTag));
    }

    Entity Player()
    {
        var player = em.CreateEntity(typeof(ActivePlayer), typeof(GhostOwnerIsLocal), typeof(SpaceLandedOn), typeof(FinalArrived));
        em.SetComponentData(player, new SpaceLandedOn { entity = property });
        return player;
    }

    int UpdateAndDrain()
    {
        popup.Update(world.Unmanaged);
        using var query = em.CreateEntityQuery(typeof(ShowActionsPanelBuffer));
        var buffer = em.GetBuffer<ShowActionsPanelBuffer>(query.GetSingletonEntity());
        int count = buffer.Length;
        if (count > 0)
        {
            Assert.AreEqual(em.GetComponentData<CurrentActivePlayer>(active).Entity, buffer[0].Player);
            Assert.AreEqual(property, buffer[0].Property);
        }
        buffer.Clear();
        return count;
    }

    [TearDown] public void Cleanup() => world.Dispose();

    [Test]
    public void SameSpaceTurnSwitchAndRepeatedSnapshotsDoNotReplayLanding()
    {
        Assert.AreEqual(1, UpdateAndDrain());
        var snapshot = em.GetComponentData<GameStateComponent>(game);
        snapshot.CompletedRounds++;
        em.SetComponentData(game, snapshot);
        Assert.AreEqual(0, UpdateAndDrain());
        em.SetComponentEnabled<ActivePlayer>(first, false);
        em.SetComponentEnabled<ActivePlayer>(second, true);
        em.SetComponentData(active, new CurrentActivePlayer { Entity = second });
        // Simulate active-player replication arriving before the shared Landing state changes.
        Assert.AreEqual(0, UpdateAndDrain());
        em.SetComponentEnabled<ActivePlayer>(second, false);
        em.SetComponentEnabled<ActivePlayer>(first, true);
        em.SetComponentData(active, new CurrentActivePlayer { Entity = first });
        Assert.AreEqual(0, UpdateAndDrain());
    }

    [Test]
    public void ActualSecondLandingOnSameSpaceShowsOnceForItsPlayer()
    {
        Assert.AreEqual(1, UpdateAndDrain());
        em.SetComponentEnabled<ActivePlayer>(first, false);
        em.SetComponentEnabled<ActivePlayer>(second, true);
        em.SetComponentData(active, new CurrentActivePlayer { Entity = second });
        em.SetComponentData(game, new GameStateComponent
        {
            State = GameState.Landing, LandingSequence = 2,
            LandingPlayer = second, LandingSpace = property
        });
        Assert.AreEqual(1, UpdateAndDrain());
        Assert.AreEqual(0, UpdateAndDrain());
    }

    [Test]
    public void OtherClientsAndStaleActiveTagsCannotReceiveLanding()
    {
        em.SetComponentEnabled<GhostOwnerIsLocal>(first, false);
        em.SetComponentEnabled<ActivePlayer>(second, true);
        Assert.AreEqual(0, UpdateAndDrain());
    }

    [Test]
    public void ServerEmitsOneIdentityPerArrivalIncludingSameSpaceRevisit()
    {
        em.CreateEntity(typeof(CharactersSpawnedTag));
        em.CreateEntity(typeof(RollAmountComponent));
        var gameplay = world.GetOrCreateSystem<GamePlaySystem>();
        em.SetComponentData(game, new GameStateComponent { State = GameState.Walking });
        em.SetComponentData(first, new FinalArrived { Value = true });
        gameplay.Update(world.Unmanaged);
        var landing = em.GetComponentData<GameStateComponent>(game);
        Assert.AreEqual(1u, landing.LandingSequence);
        Assert.AreEqual(first, landing.LandingPlayer);
        Assert.AreEqual(property, landing.LandingSpace);
        Assert.AreEqual(GameState.Landing, landing.State);
        gameplay.Update(world.Unmanaged);
        Assert.AreEqual(1u, em.GetComponentData<GameStateComponent>(game).LandingSequence);
        em.SetComponentData(first, new FinalArrived { Value = true });
        gameplay.Update(world.Unmanaged);
        Assert.AreEqual(2u, em.GetComponentData<GameStateComponent>(game).LandingSequence);
    }
}

using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.GamePlay;
using DOTS.GamePlay.PropertyAnimations;
using DOTS.GameSpaces;
using NUnit.Framework;
using Unity.Entities;

public class CardLandingTests
{
    World world;
    EntityManager em;
    SimulationSystemGroup group;
    Entity game, tile, first, second;
    bool treasure;
    void Setup(bool isTreasure)
    {
        treasure = isTreasure; world = new World("Card landing regression"); em = world.EntityManager;
        group = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
        if (treasure) group.AddSystemToUpdateList(world.CreateSystem<TreasureSystem>());
        else group.AddSystemToUpdateList(world.CreateSystem<PickRandomChanceCardSystem>());
        game = em.CreateEntity(typeof(GameStateComponent));
        var random = em.CreateEntity(typeof(RandomValueComponent));
        em.SetComponentData(random, new RandomValueComponent { Value = new Unity.Mathematics.Random(123) });
        tile = em.CreateEntity();
        if (treasure)
        {
            em.AddComponent<TreasureSpaceTag>(tile);
            em.AddBuffer<TreasureCardsBuffer>(tile).Add(new TreasureCardsBuffer { id = 1, msg = "Treasure", amount = 50 });
            em.AddBuffer<TreasureAnimationBuffer>(tile);
            em.AddComponent<CurrentTreasureAnimation>(tile); em.AddComponent<AnimationPlayState>(tile);
        }
        else
        {
            em.AddComponent<ChanceSpaceTag>(tile);
            em.AddBuffer<ChanceActionDataBuffer>(tile).Add(new ChanceActionDataBuffer { id = 1, msg = "Chance", amount = 50 });
        }
        first = Player(); second = Player(); em.SetComponentEnabled<ActivePlayer>(second, false);
    }
    Entity Player()
    {
        var e = em.CreateEntity(typeof(ActivePlayer), typeof(SpaceLandedOn), typeof(GhostMoneyComponet), typeof(LandingPaymentResolved), typeof(GhostChanceCardPicked), typeof(GhostTreasureCardPicked));
        em.SetComponentData(e, new SpaceLandedOn { entity = tile });
        em.SetComponentData(e, new GhostMoneyComponet { Value = 100 }); return e;
    }
    void LandingUpdate()
    {
        // Force a shared-state change without a new landing, like a round/snapshot update.
        em.SetComponentData(game, new GameStateComponent { State = GameState.Landing, AllPlacesInstantiated = true }); group.Update();
    }
    uint Sequence(Entity e) => treasure ? em.GetComponentData<GhostTreasureCardPicked>(e).DrawSequence : em.GetComponentData<GhostChanceCardPicked>(e).DrawSequence;
    [TearDown] public void Cleanup() { if (world != null && world.IsCreated) world.Dispose(); }
    [TestCase(false)] [TestCase(true)] public void SharedUpdatesAndReturningTurnDoNotRedraw(bool isTreasure)
    {
        Setup(isTreasure); LandingUpdate(); LandingUpdate();
        Assert.AreEqual(1u, Sequence(first)); Assert.AreEqual(150, em.GetComponentData<GhostMoneyComponet>(first).Value);
        em.SetComponentEnabled<ActivePlayer>(first, false); em.SetComponentEnabled<ActivePlayer>(second, true); LandingUpdate();
        Assert.AreEqual(1u, Sequence(second));
        em.SetComponentEnabled<ActivePlayer>(second, false); em.SetComponentEnabled<ActivePlayer>(first, true); LandingUpdate();
        Assert.AreEqual(1u, Sequence(first)); Assert.AreEqual(150, em.GetComponentData<GhostMoneyComponet>(first).Value);
    }
    [TestCase(false)] [TestCase(true)] public void NewLandingOnSameTileCanDrawIdenticalCardAgain(bool isTreasure)
    {
        Setup(isTreasure); LandingUpdate();
        em.SetComponentData(first, new LandingPaymentResolved()); LandingUpdate();
        Assert.AreEqual(2u, Sequence(first)); Assert.AreEqual(200, em.GetComponentData<GhostMoneyComponet>(first).Value);
    }
}

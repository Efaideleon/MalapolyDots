using Assets.Common;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.DataComponents;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.Authorings;
using Assets.Scripts.DOTS.Mediator;
using Assets.Scripts.DOTS.Mediator.Systems.DebugScreenSystem;
using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class SoloModeTests
{
    World world;
    EntityManager em;
    Entity ai, human, connection, game, active, deed;
    AiOpponentSystem brain;
    AssetTradingSystem trading;
    double now;

    [SetUp] public void Setup()
    {
        SoloSession.Reset(); SoloSession.Active = true;
        world = new World("Solo mode tests"); em = world.EntityManager;
        brain = world.GetOrCreateSystemManaged<AiOpponentSystem>();
        trading = world.GetOrCreateSystemManaged<AssetTradingSystem>();
        game = em.CreateSingleton(new GameStateComponent { State = GameState.Landing, AllPlacesInstantiated = true });
        ai = Player(SoloSession.FirstAiId, 1000, "Bird");
        human = Player(1, 1000, "Avocado");
        em.AddComponentData(ai, new AiOpponent { ObservedState = (int)GameState.Landing });
        active = em.CreateSingleton(new CurrentActivePlayer { Entity = ai });
        connection = em.CreateEntity(typeof(NetworkId), typeof(NetworkStreamInGame));
        em.SetComponentData(connection, new NetworkId { Value = 1 });
        deed = Deed(1, 200, Entity.Null, PropertyColor.Brown);
        em.SetComponentData(ai, new SpaceLandedOn { entity = deed });
        now = 10;
    }

    [TearDown] public void Cleanup() { world.Dispose(); SoloSession.Reset(); }

    Entity Player(int id, int cash, string name)
    {
        var player = em.CreateEntity(typeof(GhostOwner), typeof(GhostMoneyComponet), typeof(BankruptPlayer),
            typeof(NameComponent), typeof(SpaceLandedOn), typeof(LandingPaymentResolved), typeof(RemainingMoves), typeof(JailState), typeof(PlayerID), typeof(ActivePlayer));
        em.SetComponentData(player, new GhostOwner { NetworkId = id });
        em.SetComponentData(player, new GhostMoneyComponet { Value = cash });
        em.SetComponentData(player, new NameComponent { Value = name });
        em.SetComponentData(player, new PlayerID { Value = id });
        return player;
    }

    Entity Deed(int id, int price, Entity owner, PropertyColor color)
    {
        var property = em.CreateEntity(typeof(OwnerComponent), typeof(OwnerByEntityComponent), typeof(SpaceIDComponent),
            typeof(PriceComponent), typeof(GhostPriceComponent), typeof(ColorCodeComponent), typeof(HousePriceComponent),
            typeof(HouseCount), typeof(NameComponent), typeof(GhostRentComponent), typeof(PropertySpaceTag));
        em.SetComponentData(property, new SpaceIDComponent { Value = id });
        em.SetComponentData(property, new PriceComponent { Value = price });
        em.SetComponentData(property, new GhostPriceComponent { Value = price });
        em.SetComponentData(property, new ColorCodeComponent { Value = color });
        em.SetComponentData(property, new HousePriceComponent { Value = 50 });
        em.SetComponentData(property, new NameComponent { Value = "Test property" });
        Own(property, owner);
        return property;
    }

    void Own(Entity property, Entity owner)
    {
        em.SetComponentData(property, new OwnerByEntityComponent { Entity = owner });
        em.SetComponentData(property, new OwnerComponent { ID = owner == Entity.Null ? PropertyConstants.Vacant : em.GetComponentData<GhostOwner>(owner).NetworkId });
    }
    void Cash(Entity player, int amount) => em.SetComponentData(player, new GhostMoneyComponet { Value = amount });
    int Cash(Entity player) => em.GetComponentData<GhostMoneyComponet>(player).Value;
    void Think() { world.SetTime(new TimeData(now++, 1)); brain.Update(); }
    void State(GameState state) { var data = em.GetComponentData<GameStateComponent>(game); data.State = state; em.SetComponentData(game, data); }
    int Requests<T>() where T : unmanaged, IComponentData { using var query = em.CreateEntityQuery(typeof(T)); return query.CalculateEntityCount(); }

    [Test] public void AiSurvivesWithoutANetworkConnection()
    {
        world.GetOrCreateSystem<BankruptcySystem>().Update(world.Unmanaged);
        Assert.IsFalse(em.GetComponentData<BankruptPlayer>(ai).Value);
        Assert.AreNotEqual(GameState.GameOver, em.GetComponentData<GameStateComponent>(game).State);
    }

    [Test] public void AiBuysAnAffordableLandingPropertyUsingTheNormalHandler()
    {
        Think(); world.GetOrCreateSystem<PurchasePropertySystem>().Update(world.Unmanaged);
        Assert.AreEqual(ai, em.GetComponentData<OwnerByEntityComponent>(deed).Entity);
        Assert.AreEqual(800, Cash(ai));
        Assert.AreEqual(0, Requests<PurchasePropertyEventRpc>());
    }

    [Test] public void AiPreservesCashInsteadOfBuyingEveryProperty()
    {
        Cash(ai, 300); Think();
        Assert.AreEqual(0, Requests<PurchasePropertyEventRpc>());
        Assert.AreEqual(1, Requests<ChangeTurnRpc>());
    }

    [Test] public void HumanCannotUseAnUnauthenticatedRequestToBuyForAi()
    {
        var request = em.CreateEntity(typeof(PurchasePropertyEventRpc), typeof(ReceiveRpcCommandRequest));
        em.SetComponentData(request, new PurchasePropertyEventRpc { ID = 1 });
        em.SetComponentData(request, new ReceiveRpcCommandRequest { SourceConnection = connection });
        world.GetOrCreateSystem<PurchasePropertySystem>().Update(world.Unmanaged);
        Assert.AreEqual(Entity.Null, em.GetComponentData<OwnerByEntityComponent>(deed).Entity);
        Assert.AreEqual(1000, Cash(ai));
        Assert.AreEqual(1000, Cash(human));
    }

    [Test] public void AiPaysRentExactlyOnce()
    {
        Own(deed, human); em.SetComponentData(deed, new GhostRentComponent { Value = 90 });
        Think(); var rent = world.GetOrCreateSystem<PayRentSystem>(); rent.Update(world.Unmanaged);
        Think(); rent.Update(world.Unmanaged);
        Assert.AreEqual(910, Cash(ai)); Assert.AreEqual(1090, Cash(human));
        Assert.IsTrue(em.GetComponentData<LandingPaymentResolved>(ai).Value);
    }

    [Test] public void AiPaysTaxBeforeEndingItsTurn()
    {
        var tax = em.CreateEntity(typeof(TaxSpaceTag), typeof(TaxAmountComponent));
        em.SetComponentData(tax, new TaxAmountComponent { Value = 200 });
        em.SetComponentData(ai, new SpaceLandedOn { entity = tax });
        Think(); Assert.AreEqual(0, Requests<ChangeTurnRpc>());
        world.GetOrCreateSystem<PayTaxesSystem>().Update(world.Unmanaged);
        Assert.AreEqual(800, Cash(ai));
        Think(); Assert.AreEqual(1, Requests<ChangeTurnRpc>());
    }

    [Test] public void AiSellsADeedToRecoverDebtBeforeDeclaringBankruptcy()
    {
        Own(deed, ai); Cash(ai, -75); Think(); trading.Update();
        Assert.AreEqual(25, Cash(ai));
        Assert.AreEqual(Entity.Null, em.GetComponentData<OwnerByEntityComponent>(deed).Entity);
        Assert.IsFalse(em.GetComponentData<BankruptPlayer>(ai).Declared);
    }

    [Test] public void AiSellsBuildingsBeforeItsDeeds()
    {
        Own(deed, ai); Deed(2, 200, ai, PropertyColor.Brown);
        em.SetComponentData(deed, new HouseCount { Value = 1 });
        Cash(ai, -20); Think(); trading.Update();
        Assert.AreEqual(5, Cash(ai));
        Assert.AreEqual(0, em.GetComponentData<HouseCount>(deed).Value);
        Assert.AreEqual(ai, em.GetComponentData<OwnerByEntityComponent>(deed).Entity);
    }

    [Test] public void AiDeclaresBankruptcyWhenItCannotRaiseMoney()
    {
        Cash(ai, -75); Think(); trading.Update();
        Assert.IsTrue(em.GetComponentData<BankruptPlayer>(ai).Declared);
        world.GetOrCreateSystem<BankruptcySystem>().Update(world.Unmanaged);
        Assert.IsTrue(em.GetComponentData<BankruptPlayer>(ai).Value);
        Assert.AreEqual(1, em.GetComponentData<GameStateComponent>(game).WinnerNetworkId);
    }

    [Test] public void AiWaitsWhileTheHumanIsRecoveringDebt()
    {
        Cash(human, -50); Think();
        Assert.AreEqual(0, Requests<PurchasePropertyEventRpc>());
        Assert.AreEqual(0, Requests<ChangeTurnRpc>());
        Assert.IsFalse(em.GetComponentData<BankruptPlayer>(human).Declared);
    }

    [Test] public void AiBuildsEvenlyWithinACompleteSet()
    {
        Own(deed, ai); var second = Deed(2, 200, ai, PropertyColor.Brown);
        Think(); trading.Update(); Think(); trading.Update();
        Assert.AreEqual(1, em.GetComponentData<HouseCount>(deed).Value);
        Assert.AreEqual(1, em.GetComponentData<HouseCount>(second).Value);
        Assert.AreEqual(900, Cash(ai));
        Think(); Assert.AreEqual(1, Requests<ChangeTurnRpc>());
    }

    [TestCase(200, true)]
    [TestCase(350, false)]
    public void AiRespondsToHumanTradeOffers(int price, bool accepted)
    {
        Own(deed, human);
        var request = em.CreateEntity(typeof(AssetTradeRpc), typeof(ReceiveRpcCommandRequest));
        em.SetComponentData(request, new AssetTradeRpc { Action = AssetAction.Offer, PropertyId = 1, BuyerId = SoloSession.FirstAiId, Price = price });
        em.SetComponentData(request, new ReceiveRpcCommandRequest { SourceConnection = connection });
        trading.Update(); Assert.AreEqual(1, Requests<AiTradeOffer>());
        Think(); trading.Update();
        Assert.AreEqual(accepted ? ai : human, em.GetComponentData<OwnerByEntityComponent>(deed).Entity);
        Assert.AreEqual(accepted ? 1000 - price : 1000, Cash(ai));
        Assert.AreEqual(accepted ? 1000 + price : 1000, Cash(human));
    }

    [Test] public void AiDiceAdvanceAfterTheAnimationWithoutAnAiClientAcknowledgement()
    {
        State(GameState.Rolling);
        em.CreateSingleton(new RollConfig());
        var roll = world.GetOrCreateSystem<RollSystem>();
        GameplayActionSource.Queue(em, ai, new RollEventRpc()); roll.Update(world.Unmanaged);
        using var query = em.CreateEntityQuery(typeof(PendingDiceRoll));
        var pending = query.GetSingleton<PendingDiceRoll>();
        Assert.IsTrue(pending.Active); Assert.IsTrue(pending.Settled);
        Assert.That(pending.First, Is.InRange(1, 6)); Assert.That(pending.Second, Is.InRange(1, 6));
        world.SetTime(new TimeData(pending.EarliestMoveTime - 0.01, 0.1f)); roll.Update(world.Unmanaged);
        Assert.AreEqual(GameState.Rolling, em.GetComponentData<GameStateComponent>(game).State);
        world.SetTime(new TimeData(pending.EarliestMoveTime, 0.1f)); roll.Update(world.Unmanaged);
        Assert.AreEqual(GameState.Walking, em.GetComponentData<GameStateComponent>(game).State);
        Assert.AreEqual(pending.First + pending.Second, em.GetComponentData<RemainingMoves>(ai).Value);
    }

    [Test] public void AiInJailEndsItsTurnInsteadOfRolling()
    {
        State(GameState.Rolling); em.SetComponentData(ai, new JailState { InJail = true, TurnsInJail = 2 });
        Think(); Think();
        Assert.AreEqual(1, Requests<ChangeTurnRpc>()); Assert.AreEqual(0, Requests<RollEventRpc>());
    }

    [Test] public void AiGoToJailUsesTheJailHandler()
    {
        var space = em.CreateEntity(typeof(GoToJailTag)); em.SetComponentData(ai, new SpaceLandedOn { entity = space });
        Think(); Assert.AreEqual(1, Requests<GoToJailRpc>()); Assert.AreEqual(0, Requests<ChangeTurnRpc>());
    }

    [TestCase(false)] [TestCase(true)] public void AiEndTurnAdvancesToTheHuman(bool jailFreePass)
    {
        var turn = world.GetOrCreateSystem<ChangeTurnSystem>();
        em.CreateSingleton(new GeneralGhostStates());
        var list = em.CreateSingletonBuffer<PlayersSortedByNetId>();
        em.GetBuffer<PlayersSortedByNetId>(list).Add(new PlayersSortedByNetId { Name = "Bird" });
        em.GetBuffer<PlayersSortedByNetId>(list).Add(new PlayersSortedByNetId { Name = "Avocado" });
        em.CreateSingleton(new CurrentPlayerID { Value = SoloSession.FirstAiId });
        em.CreateSingleton(new CurrentPlayerComponent { entity = ai });
        em.SetComponentEnabled<ActivePlayer>(human, false);
        if (jailFreePass) em.SetComponentData(human, new JailState { InJail = true, TurnsInJail = 1, HasGetOutOfJailFreeCard = true });
        Cash(ai, 300); Think(); turn.Update(world.Unmanaged);
        Assert.AreEqual(human, em.GetComponentData<CurrentActivePlayer>(active).Entity);
        Assert.AreEqual(GameState.Rolling, em.GetComponentData<GameStateComponent>(game).State);
        Assert.IsFalse(em.GetComponentData<JailState>(human).InJail);
        Assert.IsFalse(em.GetComponentData<JailState>(human).HasGetOutOfJailFreeCard);
    }

    [TestCase(1)] [TestCase(5)]
    public void SoloSetupReservesUniqueTokensAndKeepsTheHumanChoice(int count)
    {
        SoloSession.Opponents = count;
        var prefab = em.CreateEntity();
        for (int i = 1; i <= 6; i++)
        {
            var choice = em.CreateEntity(typeof(PrepickedCharacter));
            em.SetComponentData(choice, new PrepickedCharacter { Character = (CharactersEnum)i, Prefab = prefab });
        }
        Assert.IsTrue(SoloOpponentSetup.Prepare(em, 1, CharactersEnum.Lira));
        using var choices = em.CreateEntityQuery(typeof(PrepickedCharacter));
        using var all = choices.ToComponentDataArray<PrepickedCharacter>(Unity.Collections.Allocator.Temp);
        int bots = 0, people = 0;
        foreach (var choice in all)
        {
            if (!choice.PrePicked) continue;
            if (choice.OwnerNetworkId == 1) { people++; Assert.AreEqual(CharactersEnum.Lira, choice.Character); }
            else { bots++; Assert.AreNotEqual(CharactersEnum.Lira, choice.Character); Assert.That(choice.OwnerNetworkId, Is.InRange(1001, 1000 + count)); }
        }
        Assert.AreEqual(count, bots); Assert.AreEqual(1, people);
    }

    [Test] public void PauseStopsAiAndRestoresSystemAndTimeSettings()
    {
        var initialization = world.GetOrCreateSystemManaged<InitializationSystemGroup>();
        var simulation = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
        var presentation = world.GetOrCreateSystemManaged<PresentationSystemGroup>();
        presentation.Enabled = false;
        float scale = Time.timeScale; bool audio = AudioListener.pause;
        using var pause = new SoloPauseState();
        ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
        try
        {
            pause.Pause(new[] { world });
            Assert.IsFalse(ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(world));
            Assert.IsTrue(initialization.Enabled); Assert.IsTrue(simulation.Enabled); Assert.IsFalse(presentation.Enabled);
            Assert.AreEqual(0, Time.timeScale); Assert.IsTrue(AudioListener.pause);
            Think(); Assert.AreEqual(0, Requests<PurchasePropertyEventRpc>());
            pause.Resume(); pause.Resume();
            Assert.IsTrue(ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(world));
            Assert.IsTrue(initialization.Enabled); Assert.IsTrue(simulation.Enabled); Assert.IsFalse(presentation.Enabled);
            Assert.AreEqual(scale, Time.timeScale); Assert.AreEqual(audio, AudioListener.pause);
            Think(); Assert.AreEqual(1, Requests<PurchasePropertyEventRpc>());
        }
        finally
        {
            pause.Resume();
            ScriptBehaviourUpdateOrder.RemoveWorldFromCurrentPlayerLoop(world);
        }
    }

    [Test] public void ResumeDoesNotRegisterAnUnscheduledWorld()
    {
        Assert.IsFalse(ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(world));
        using var pause = new SoloPauseState();
        pause.Pause(new[] { world });
        pause.Resume();
        Assert.IsFalse(ScriptBehaviourUpdateOrder.IsWorldInCurrentPlayerLoop(world));
    }
}

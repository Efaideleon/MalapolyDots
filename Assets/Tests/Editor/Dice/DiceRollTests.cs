using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.Mediator.Systems.DebugScreenSystem;
using DOTS.GamePlay;
using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class DiceRollTests
{
    World world;
    EntityManager manager;
    SimulationSystemGroup group;
    Entity player, connection, game, config, active;

    [SetUp] public void Setup()
    {
        world = new World("Dice roll tests");
        manager = world.EntityManager;
        group = world.GetOrCreateSystemManaged<SimulationSystemGroup>();
        group.AddSystemToUpdateList(world.CreateSystem<RollSystem>());
        player = manager.CreateEntity(typeof(GhostOwner), typeof(RemainingMoves));
        manager.SetComponentData(player, new GhostOwner { NetworkId = 7 });
        connection = manager.CreateEntity(typeof(NetworkId));
        manager.SetComponentData(connection, new NetworkId { Value = 7 });
        active = manager.CreateEntity(typeof(CurrentActivePlayer));
        manager.SetComponentData(active, new CurrentActivePlayer { Entity = player });
        game = manager.CreateEntity(typeof(GameStateComponent));
        manager.SetComponentData(game, new GameStateComponent { State = GameState.Rolling });
        config = manager.CreateEntity(typeof(RollConfig));
        manager.SetComponentData(config, new RollConfig { isCustomEnabled = true, customRollValue = 9 });
    }

    [TearDown] public void Cleanup() => world.Dispose();
    T Singleton<T>() where T : unmanaged, IComponentData => manager.CreateEntityQuery(typeof(T)).GetSingleton<T>();
    void Tick(double time)
    {
        world.SetTime(new TimeData(time, .016f));
        group.Update();
    }
    Entity Request(Entity source)
    {
        var request = manager.CreateEntity(typeof(RollEventRpc), typeof(ReceiveRpcCommandRequest));
        manager.SetComponentData(request, new ReceiveRpcCommandRequest { SourceConnection = source });
        return request;
    }
    void Ack(Entity source, uint sequence)
    {
        var ack = manager.CreateEntity(typeof(DiceSettledRpc), typeof(ReceiveRpcCommandRequest));
        manager.SetComponentData(ack, new DiceSettledRpc { Sequence = sequence });
        manager.SetComponentData(ack, new ReceiveRpcCommandRequest { SourceConnection = source });
    }

    [Test] public void MovementWaitsForChosenDiceAndOwnerAcknowledgement()
    {
        Request(connection); Tick(1);
        var pending = Singleton<PendingDiceRoll>();
        var result = Singleton<DiceRollResultRpc>();
        Assert.AreEqual(9, result.First + result.Second);
        Assert.AreEqual(pending.First, result.First);
        Assert.AreEqual(pending.Second, result.Second);
        Assert.AreEqual(0, manager.GetComponentData<RemainingMoves>(player).Value);
        Tick(4);
        Assert.AreEqual(GameState.Rolling, manager.GetComponentData<GameStateComponent>(game).State);
        Ack(connection, pending.Sequence); Tick(4.1);
        Assert.AreEqual(GameState.Walking, manager.GetComponentData<GameStateComponent>(game).State);
        Assert.AreEqual(9, manager.GetComponentData<RemainingMoves>(player).Value);
        Assert.AreEqual(result.First, Singleton<RollAmountComponent>().FirstDie);
        Assert.AreEqual(result.Second, Singleton<RollAmountComponent>().SecondDie);
    }

    [Test] public void EarlyAcknowledgementCannotSkipAnimation()
    {
        Request(connection); Tick(1);
        Ack(connection, Singleton<PendingDiceRoll>().Sequence); Tick(1.1);
        Assert.AreEqual(0, manager.GetComponentData<RemainingMoves>(player).Value);
        Tick(1 + DiceRollValues.AnimationSeconds + .1);
        Assert.AreEqual(9, manager.GetComponentData<RemainingMoves>(player).Value);
    }

    [Test] public void DuplicateRequestsAreConsumedWithoutRerolling()
    {
        var first = Request(connection); var second = Request(connection); Tick(1);
        Assert.IsFalse(manager.Exists(first)); Assert.IsFalse(manager.Exists(second));
        Assert.AreEqual(1, manager.CreateEntityQuery(typeof(DiceRollResultRpc)).CalculateEntityCount());
        var again = Request(connection); Tick(2);
        Assert.IsFalse(manager.Exists(again));
        Assert.AreEqual(1u, Singleton<PendingDiceRoll>().Sequence);
    }

    [Test] public void UnauthorizedRequestsAndAcknowledgementsAreIgnoredAndConsumed()
    {
        var stranger = manager.CreateEntity(typeof(NetworkId));
        manager.SetComponentData(stranger, new NetworkId { Value = 8 });
        var request = Request(stranger); Tick(1);
        Assert.IsFalse(manager.Exists(request)); Assert.IsFalse(Singleton<PendingDiceRoll>().Active);
        Request(connection); Tick(2);
        Ack(stranger, Singleton<PendingDiceRoll>().Sequence); Tick(5);
        Assert.IsFalse(Singleton<PendingDiceRoll>().Settled);
        Assert.AreEqual(0, manager.GetComponentData<RemainingMoves>(player).Value);
    }

    [Test] public void MissingAcknowledgementTimesOutWithoutStallingTurn()
    {
        Request(connection); Tick(1); Tick(9.1);
        Assert.AreEqual(9, manager.GetComponentData<RemainingMoves>(player).Value);
        Assert.IsFalse(Singleton<PendingDiceRoll>().Active);
    }

    [Test] public void TurnChangeCancelsPendingRoll()
    {
        Request(connection); Tick(1);
        manager.SetComponentData(active, new CurrentActivePlayer { Entity = Entity.Null });
        Tick(10);
        Assert.IsFalse(Singleton<PendingDiceRoll>().Active);
        Assert.AreEqual(0, manager.GetComponentData<RemainingMoves>(player).Value);
    }

    [Test] public void EveryCustomTotalProducesLegalFaces()
    {
        var random = new Unity.Mathematics.Random(123);
        for (int total = -2; total <= 16; total++)
        for (int attempt = 0; attempt < 20; attempt++)
        {
            var pair = DiceRollValues.FromTotal(total, ref random);
            Assert.That(pair.x, Is.InRange(1, 6)); Assert.That(pair.y, Is.InRange(1, 6));
            Assert.AreEqual(Mathf.Clamp(total, 2, 12), pair.x + pair.y);
        }
    }

    [Test] public void ImportedPrefabLandsOnAllThirtySixPairs()
    {
        var prefab = Resources.Load<DiceRollPresenter>("Dice/DiceRollPresentation");
        Assert.IsNotNull(prefab);
        var presenter = Object.Instantiate(prefab);
        presenter.FollowCamera = false;
        try
        {
            for (int a = 1; a <= 6; a++)
            for (int b = 1; b <= 6; b++)
            {
                presenter.Play(a, b, (uint)(a * 6 + b));
                presenter.FirstDie.Sample(1); presenter.SecondDie.Sample(1);
                Assert.Greater(Vector3.Dot(presenter.FirstDie.transform.localRotation * presenter.FirstDie.FaceNormal(a), Vector3.up), .99999f);
                Assert.Greater(Vector3.Dot(presenter.SecondDie.transform.localRotation * presenter.SecondDie.FaceNormal(b), Vector3.up), .99999f);
                Assert.That(presenter.FirstDie.transform.localPosition.y, Is.EqualTo(.5f).Within(.001));
                Assert.That(presenter.SecondDie.transform.localPosition.y, Is.EqualTo(.5f).Within(.001));
            }
            Assert.Throws<System.ArgumentOutOfRangeException>(() => presenter.Play(0, 7));
        }
        finally { Object.DestroyImmediate(presenter.gameObject); }
    }

    [Test] public void ClientPresentsServerFacesAndAcknowledgesOnlyAfterSettling()
    {
        manager.AddComponent<NetworkStreamInGame>(connection);
        var client = world.GetOrCreateSystemManaged<DiceRollClientSystem>();
        var message = manager.CreateEntity(typeof(DiceRollResultRpc), typeof(ReceiveRpcCommandRequest));
        manager.SetComponentData(message, new DiceRollResultRpc { First = 2, Second = 6, Sequence = 77 });
        client.Update();
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var presenter = (DiceRollPresenter)typeof(DiceRollClientSystem).GetField("presenter", flags).GetValue(client);
        try
        {
            Assert.IsFalse(manager.Exists(message));
            Assert.IsNotNull(presenter);
            Assert.IsTrue(presenter.IsRolling);
            Assert.AreEqual(2, presenter.FirstDie.Value);
            Assert.AreEqual(6, presenter.SecondDie.Value);
            Assert.AreEqual(0, manager.CreateEntityQuery(typeof(DiceSettledRpc)).CalculateEntityCount());
            typeof(DiceRollPresenter).GetField("elapsed", flags).SetValue(presenter, DiceRollValues.FloorRollSeconds);
            typeof(DiceRollPresenter).GetMethod("LateUpdate", flags).Invoke(presenter, null);
            client.Update();
            Assert.IsTrue(presenter.IsRolling, "Landing alone must not release movement before the screen reveal.");
            Assert.AreEqual(0, manager.CreateEntityQuery(typeof(DiceSettledRpc)).CalculateEntityCount());
            // Advance the presentation clock, then execute the same final-frame code as Play mode.
            typeof(DiceRollPresenter).GetField("elapsed", flags).SetValue(presenter, DiceRollValues.AnimationSeconds);
            typeof(DiceRollPresenter).GetMethod("LateUpdate", flags).Invoke(presenter, null);
            Assert.IsFalse(presenter.IsRolling);
            client.Update();
            Assert.AreEqual(77u, Singleton<DiceSettledRpc>().Sequence);
            Assert.Greater(Vector3.Dot(presenter.FirstDie.transform.localRotation * presenter.FirstDie.FaceNormal(2), Vector3.up), .99999f);
            Assert.Greater(Vector3.Dot(presenter.SecondDie.transform.localRotation * presenter.SecondDie.FaceNormal(6), Vector3.up), .99999f);
        }
        finally { if (presenter != null) Object.DestroyImmediate(presenter.gameObject); }
    }

    [Test] public void FloorRollStaysInWorldThenAllFacesPullTowardCamera()
    {
        var cameraObject = new GameObject("Dice sequence test camera");
        var camera = cameraObject.AddComponent<Camera>();
        camera.transform.position = new Vector3(0, 8, -12);
        camera.transform.LookAt(new Vector3(0, 1, 0));
        var presenter = Object.Instantiate(Resources.Load<DiceRollPresenter>("Dice/DiceRollPresentation"));
        presenter.PresentationCamera = camera;
        presenter.FloorHeight = 1;
        try
        {
            for (int a = 1; a <= 6; a++)
            for (int b = 1; b <= 6; b++)
            {
                presenter.Play(a, b, (uint)(a * 6 + b));
                Vector3 floorPosition = presenter.transform.position;
                Assert.That(floorPosition.y, Is.EqualTo(1).Within(.001));
                Assert.Greater(presenter.FirstDie.transform.localPosition.y, 3);
                camera.transform.position += Vector3.right * .02f;
                presenter.SampleSequence(DiceRollValues.FloorRollSeconds);
                Assert.That(Vector3.Distance(floorPosition, presenter.transform.position), Is.LessThan(.001));
                foreach (var die in new[] { presenter.FirstDie, presenter.SecondDie })
                {
                    Assert.Greater(Vector3.Dot(die.transform.TransformDirection(die.FaceNormal(die.Value)), Vector3.up), .9999f);
                    Assert.IsTrue(die.ContactShadow.gameObject.activeSelf);
                }
                presenter.SampleSequence(DiceRollValues.FloorRollSeconds + DiceRollValues.FloorPauseSeconds);
                Vector3 beforePull = presenter.FirstDie.transform.position;
                presenter.SampleSequence(DiceRollValues.FloorRollSeconds + DiceRollValues.FloorPauseSeconds + .0001f);
                Assert.That(Vector3.Distance(beforePull, presenter.FirstDie.transform.position), Is.LessThan(.01), "Pull should start continuously.");
                presenter.SampleSequence(DiceRollValues.AnimationSeconds);
                foreach (var die in new[] { presenter.FirstDie, presenter.SecondDie })
                {
                    Assert.Greater(Vector3.Dot(die.transform.TransformDirection(die.FaceNormal(die.Value)), -camera.transform.forward), .98f);
                    var viewport = camera.WorldToViewportPoint(die.transform.position);
                    Assert.That(viewport.x, Is.InRange(.2f, .8f));
                    Assert.That(viewport.y, Is.EqualTo(.57f).Within(.01));
                    Assert.IsFalse(die.ContactShadow.gameObject.activeSelf, "Floor shadows must not follow floating dice.");
                }
            }
        }
        finally
        {
            Object.DestroyImmediate(presenter.gameObject);
            Object.DestroyImmediate(cameraObject);
        }
    }
}

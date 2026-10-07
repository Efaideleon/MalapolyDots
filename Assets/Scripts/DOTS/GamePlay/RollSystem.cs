using DOTS.EventBuses;
using Unity.Burst;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using Assets.Scripts.DOTS.GamePlay;
using Unity.NetCode;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.Mediator.Systems.DebugScreenSystem;

namespace DOTS.GamePlay
{
    public struct RollAmountComponent : IComponentData
    {
        public int Value;
        public int FirstDie, SecondDie;
    }

    public struct RandomValueComponent : IComponentData
    {
        public Random Value;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct RollSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            uint seed = (uint)System.Environment.TickCount;
            state.EntityManager.CreateSingleton(new RandomValueComponent { Value = new Random(seed == 0 ? 1u : seed) });
            state.EntityManager.CreateSingleton(new RollAmountComponent { Value = default });
            state.EntityManager.CreateSingleton(new PendingDiceRoll());
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameStateComponent>();
#if UNITY_EDITOR
            state.RequireForUpdate<RollConfig>();
#endif
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            var pending = SystemAPI.GetSingleton<PendingDiceRoll>();
            var activePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            bool hasDebt = false;
            foreach (var (money, bankruptcy) in SystemAPI.Query<RefRO<GhostMoneyComponet>, RefRO<BankruptPlayer>>())
                hasDebt |= !bankruptcy.ValueRO.Value && money.ValueRO.Value < 0;
            double now = SystemAPI.Time.ElapsedTime;

            foreach (var (ack, source, entity) in SystemAPI.Query<RefRO<DiceSettledRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                if (pending.Active && ack.ValueRO.Sequence == pending.Sequence &&
                    SystemAPI.HasComponent<NetworkId>(source.ValueRO.SourceConnection) &&
                    SystemAPI.HasComponent<GhostOwner>(pending.Player) &&
                    SystemAPI.GetComponent<NetworkId>(source.ValueRO.SourceConnection).Value ==
                    SystemAPI.GetComponent<GhostOwner>(pending.Player).NetworkId)
                    pending.Settled = true;
                ecb.DestroyEntity(entity);
            }

            if (pending.Active && !hasDebt)
            {
                if (activePlayer != pending.Player || game.State != GameState.Rolling ||
                    !SystemAPI.HasComponent<RemainingMoves>(pending.Player))
                    pending.Active = false;
                else if ((pending.Settled && now >= pending.EarliestMoveTime) || now >= pending.Timeout)
                {
                    int total = pending.First + pending.Second;
                    SystemAPI.SetSingleton(new RollAmountComponent { Value = total, FirstDie = pending.First, SecondDie = pending.Second });
                    SystemAPI.GetComponentRW<RemainingMoves>(pending.Player).ValueRW.Value = total;
                    game.State = GameState.Walking;
                    SystemAPI.SetSingleton(game);
                    pending.Active = false;
                }
            }

            foreach (var (rpc, _, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<RollEventRpc>>().WithEntityAccess())
            {
                // Consume every request, including duplicates and unauthorized requests.
                ecb.DestroyEntity(entity);
                if (hasDebt || pending.Active || game.State != GameState.Rolling ||
                    !SystemAPI.HasComponent<GhostOwner>(activePlayer) ||
                    !SystemAPI.HasComponent<RemainingMoves>(activePlayer) ||
                    !GameplayActionSource.Owns(state.EntityManager, entity, rpc.ValueRO.SourceConnection, activePlayer)) continue;

                if (SystemAPI.HasComponent<LandingPaymentResolved>(activePlayer))
                    SystemAPI.SetComponent(activePlayer, new LandingPaymentResolved());
                var randomData = SystemAPI.GetSingletonRW<RandomValueComponent>();
                int first = randomData.ValueRW.Value.NextInt(1, 7);
                int second = randomData.ValueRW.Value.NextInt(1, 7);
#if UNITY_EDITOR
                var config = SystemAPI.GetSingleton<RollConfig>();
                if (config.isCustomEnabled)
                {
                    var pair = DiceRollValues.FromTotal(config.customRollValue, ref randomData.ValueRW.Value);
                    first = pair.x;
                    second = pair.y;
                }
#endif
                uint sequence = pending.Sequence + 1;
                if (sequence == 0) sequence = 1;
                pending = new PendingDiceRoll
                {
                    Active = true, Player = activePlayer, First = first, Second = second, Sequence = sequence,
                    Settled = SystemAPI.HasComponent<AiOpponent>(activePlayer),
                    EarliestMoveTime = now + DiceRollValues.AnimationSeconds,
                    // A disconnected or headless client must never leave a turn stuck forever.
                    Timeout = now + 8
                };
                var message = ecb.CreateEntity();
                ecb.AddComponent(message, new DiceRollResultRpc { First = first, Second = second, Sequence = sequence });
                ecb.AddComponent<SendRpcCommandRequest>(message);
            }
            SystemAPI.SetSingleton(pending);
            ecb.Playback(state.EntityManager);
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct RouteRollEventToServer : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RollEventBuffer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<RollEventBuffer>>().WithChangeFilter<RollEventBuffer>())
            {
                if (buffer.Length < 1)
                    continue;

                foreach (var _ in buffer)
                {
                    var entity = ecb.CreateEntity();
                    ecb.AddComponent<RollEventRpc>(entity);
                    ecb.AddComponent<SendRpcCommandRequest>(entity);
                }

                buffer.Clear();
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct RollEventRpc : IRpcCommand
    { }
}

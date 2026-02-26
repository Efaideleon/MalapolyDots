using DOTS.EventBuses;
using Unity.Burst;
using Unity.Entities;
using Random = Unity.Mathematics.Random;
using DOTS.GamePlay.DebugAuthoring;
using Assets.Scripts.DOTS.GamePlay;
using Unity.NetCode;
using Assets.Scripts.DOTS.Characters;

namespace DOTS.GamePlay
{
    public struct RollAmountComponent : IComponentData
    {
        public int Value;
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
            state.EntityManager.CreateSingleton(new RandomValueComponent { Value = new Random(seed) });
            state.EntityManager.CreateSingleton(new RollAmountComponent { Value = default });
            state.RequireForUpdate<CurrentActivePlayer>();
#if UNITY_EDITOR
            state.RequireForUpdate<RollConfig>();
#endif
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (rpc, _, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<RollEventRpc>>().WithEntityAccess())
            {
                var activePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
                if (activePlayer == default)
                {
                    ecb.DestroyEntity(entity);
                    return;
                }

                var clientId = SystemAPI.GetComponent<NetworkId>(rpc.ValueRO.SourceConnection);
                var ghostOwnerId = SystemAPI.GetComponent<GhostOwner>(activePlayer);
                if (clientId.Value != ghostOwnerId.NetworkId)
                {
                    return;
                }

                var rollAmount = SystemAPI.GetSingletonRW<RollAmountComponent>();
                var randomData = SystemAPI.GetSingletonRW<RandomValueComponent>();
                rollAmount.ValueRW.Value = randomData.ValueRW.Value.NextInt(1, 7);

#if UNITY_EDITOR
                var rollConfig = SystemAPI.GetSingleton<RollConfig>();
                var isCustomEnabled = rollConfig.isCustomEnabled;
                UnityEngine.Debug.Log($"[RollSystem] | rollConfig.isCustomEnabled = {isCustomEnabled}");
                var customRollValue = rollConfig.customRollValue;
                rollAmount.ValueRW.Value = isCustomEnabled ? customRollValue : rollAmount.ValueRO.Value;
#endif
                SystemAPI.GetComponentRW<RemainingMoves>(activePlayer).ValueRW.Value = rollAmount.ValueRO.Value;
                UnityEngine.Debug.Log($"[RollSystem] | Roll event processed in the server amount: {rollAmount.ValueRO.Value}.");
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
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

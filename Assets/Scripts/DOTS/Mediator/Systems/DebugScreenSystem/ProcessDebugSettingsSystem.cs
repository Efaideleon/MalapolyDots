using DOTS.Mediator.Systems.DebugScreenSystem;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.Mediator.Systems.DebugScreenSystem
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ProcessDebugSettingsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<RollConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (toggleCustomRoll, _, entity) in SystemAPI.Query<RefRO<ToggleCustomRollRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                var config = SystemAPI.GetSingletonRW<RollConfig>();
                config.ValueRW.isCustomEnabled = toggleCustomRoll.ValueRO.Value;
                ecb.DestroyEntity(entity);
            }

            foreach (var (customRoll, _, entity) in SystemAPI.Query<RefRO<CustomRollValueRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                var config = SystemAPI.GetSingletonRW<RollConfig>();
                config.ValueRW.customRollValue = customRoll.ValueRO.Value;
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

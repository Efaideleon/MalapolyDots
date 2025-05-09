using Unity.Burst;
using Unity.Entities;

namespace DOTS.EventBuses
{
    public struct UIButtonEventBus : IBufferElementData
    { }

    [BurstCompile]
    public partial struct UIEventBusSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<UIButtonEventBus>();
            state.RequireForUpdate<UIButtonDirtyFlag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<UIButtonEventBus>>().WithChangeFilter<UIButtonEventBus>())
            {
                foreach (var _ in buffer)
                {
                    SystemAPI.GetSingletonRW<UIButtonDirtyFlag>().ValueRW.Value = true;
                }
                buffer.Clear();
            }
        }
    }
}

using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Mediator
{
    public partial struct DigitsToUVSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new NumberToUVOffset { Map = new(12, Allocator.Persistent) });
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            var numberToUVOffset = SystemAPI.GetSingletonRW<NumberToUVOffset>();
            numberToUVOffset.ValueRW.Map.TryAdd(0, new float2(0, 0.75f));
            numberToUVOffset.ValueRW.Map.TryAdd(1, new float2(0.25f, 0.75f));
            numberToUVOffset.ValueRW.Map.TryAdd(2, new float2(0.5f, 0.75f));
            numberToUVOffset.ValueRW.Map.TryAdd(3, new float2(0.75f, 0.75f));
            numberToUVOffset.ValueRW.Map.TryAdd(4, new float2(0f, 0.5f));
            numberToUVOffset.ValueRW.Map.TryAdd(5, new float2(0.25f, 0.5f));
            numberToUVOffset.ValueRW.Map.TryAdd(6, new float2(0.5f, 0.5f));
            numberToUVOffset.ValueRW.Map.TryAdd(7, new float2(0.75f, 0.5f));
            numberToUVOffset.ValueRW.Map.TryAdd(8, new float2(0, 0.25f));
            numberToUVOffset.ValueRW.Map.TryAdd(9, new float2(0.25f, 0.25f));
            numberToUVOffset.ValueRW.Map.TryAdd(10, new float2(0.5f, 0.25f));  // Q
            numberToUVOffset.ValueRW.Map.TryAdd(11, new float2(0.75f, 0.25f)); // ,
        }

        public void OnDestroy(ref SystemState state)
        {
            var numberToUVOffset = SystemAPI.GetSingletonRW<NumberToUVOffset>();
            if (numberToUVOffset.ValueRW.Map.IsCreated)
            {
                numberToUVOffset.ValueRW.Map.Clear();
            }
        }
    }

    public struct NumberToUVOffset : IComponentData
    {
        public NativeHashMap<int, float2> Map;
    }
}

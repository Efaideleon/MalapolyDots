using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.Mediator
{
    public partial struct DigitsToUVSystem : ISystem
    {
        private NativeHashMap<int, float2> map;
        public void OnCreate(ref SystemState state)
        {
            map = new(12, Allocator.Persistent);
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            map.TryAdd(0, new float2(0, 0.75f));
            map.TryAdd(1, new float2(0.25f, 0.75f));
            map.TryAdd(2, new float2(0.5f, 0.75f));
            map.TryAdd(3, new float2(0.75f, 0.75f));
            map.TryAdd(4, new float2(0f, 0.5f));
            map.TryAdd(5, new float2(0.25f, 0.5f));
            map.TryAdd(6, new float2(0.5f, 0.5f));
            map.TryAdd(7, new float2(0.75f, 0.5f));
            map.TryAdd(8, new float2(0, 0.25f));
            map.TryAdd(9, new float2(0.25f, 0.25f));
            map.TryAdd(10, new float2(0.5f, 0.25f));  // Q
            map.TryAdd(11, new float2(0.75f, 0.25f)); // ,

            state.EntityManager.CreateSingleton(new NumberToUVOffset { Map = map });
        }

        public void OnDestroy(ref SystemState state)
        {
            if (map.IsCreated)
            {
                map.Clear();
                map.Dispose();
            }
        }
    }

    public struct NumberToUVOffset : IComponentData
    {
        public NativeHashMap<int, float2> Map;
    }
}

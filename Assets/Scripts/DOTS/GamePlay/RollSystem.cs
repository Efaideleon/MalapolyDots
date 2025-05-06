using DOTS.EventBuses;
using Unity.Burst;
using Unity.Entities;
using Random = Unity.Mathematics.Random;

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

    public partial struct RollSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            uint seed = (uint)System.Environment.TickCount;  
            state.EntityManager.CreateSingleton(new RandomValueComponent { Value = new Random(seed) });
            state.EntityManager.CreateSingleton(new RollAmountComponent { Value = default });
            state.RequireForUpdate<RollAmountComponent>();
            state.RequireForUpdate<RollEventBuffer>();
            state.RequireForUpdate<RandomValueComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<RollEventBuffer>>().WithChangeFilter<RollEventBuffer>()) 
            {
                if (buffer.Length < 1)
                    continue;

                foreach (var _ in buffer)
                {
                    var rollAmount = SystemAPI.GetSingletonRW<RollAmountComponent>();
                    var randomData = SystemAPI.GetSingletonRW<RandomValueComponent>();
                    //rollAmount.ValueRW.Value = randomData.ValueRW.Value.NextInt(1, 7);
                    rollAmount.ValueRW.Value = 5;
                }

                buffer.Clear();
            }
        }
    }
}

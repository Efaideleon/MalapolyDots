using Unity.Burst;
using Unity.Entities;
using Random = Unity.Mathematics.Random;

public struct RollAmountComponent : IComponentData
{
    public int AmountRolled;
}

public struct RandomValueComponent : IComponentData
{
    public Random Value;
}

public struct RollEventBuffer : IBufferElementData { }

public partial struct RollSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        uint seed = (uint)System.Environment.TickCount;  
        state.EntityManager.CreateSingleton(new RandomValueComponent { Value = new Random(seed) });
        state.EntityManager.CreateSingleton(new RollAmountComponent { AmountRolled = default });
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
                var rollPanelComponent = SystemAPI.GetSingletonRW<RollAmountComponent>();
                var randomData = SystemAPI.GetSingletonRW<RandomValueComponent>();
                rollPanelComponent.ValueRW.AmountRolled = randomData.ValueRW.Value.NextInt(1, 7);
            }

            buffer.Clear();
        }
    }
}

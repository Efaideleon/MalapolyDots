using Unity.Burst;
using Unity.Entities;

public struct LastNumberOfRoundsClicked : IComponentData
{
    public int Value;
}

[BurstCompile]
public partial struct LastNumberOfRoundsClickedSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingleton<LastNumberOfRoundsClicked>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var eventBuffer in 
                SystemAPI.Query<
                    DynamicBuffer<NumberOfRoundsEventBuffer>
                >()
                .WithChangeFilter<NumberOfRoundsEventBuffer>())
        {
            foreach (var e in eventBuffer)
            {
                SystemAPI.GetSingletonRW<LastNumberOfRoundsClicked>().ValueRW.Value = e.NumberOfRounds;
            }
            eventBuffer.Clear();
        }
    }
}

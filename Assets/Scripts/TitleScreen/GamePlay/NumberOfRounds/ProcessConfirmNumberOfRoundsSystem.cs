using Unity.Burst;
using Unity.Entities;

public struct NumberOfRoundsConfirmEventBuffer : IBufferElementData { }

[BurstCompile]
public partial struct ProcessConfirmNumberOfRoundsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingletonBuffer<NumberOfRoundsConfirmEventBuffer>();
        state.RequireForUpdate<LoginData>();
        state.RequireForUpdate<LastNumberOfRoundsClicked>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var buffer in 
                SystemAPI.Query<
                DynamicBuffer<NumberOfRoundsConfirmEventBuffer>
                >()
                .WithChangeFilter<NumberOfRoundsConfirmEventBuffer>())
        {
            foreach (var _ in buffer)
            {
                var numOfRounds = SystemAPI.GetSingleton<LastNumberOfRoundsClicked>().Value;
                SystemAPI.GetSingletonRW<LoginData>().ValueRW.NumberOfRounds = numOfRounds;
            }
            buffer.Clear();
        }
    }
}

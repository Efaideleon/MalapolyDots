using Unity.Entities;

public struct RollAmountCountDown : IComponentData
{
    public int Value;
}

public partial struct RollAmountCountDownSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RollAmountCountDown>();
        state.EntityManager.CreateSingleton(new RollAmountCountDown { Value = default });
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var _ in SystemAPI.Query<RefRO<PlayerWaypointIndex>>().WithChangeFilter<PlayerWaypointIndex>())
        {
            SystemAPI.GetSingletonRW<RollAmountCountDown>().ValueRW.Value -= 1;
            break;
        }
    }
}

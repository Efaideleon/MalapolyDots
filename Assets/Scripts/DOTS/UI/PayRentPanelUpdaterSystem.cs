using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct PayRentPanelUpdaterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PayRentPanelContextComponent>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ( var clickedProperty in 
                SystemAPI.Query<
                    RefRO<LastPropertyClicked>
                >().
                WithChangeFilter<LastPropertyClicked>())
        {
            if (clickedProperty.ValueRO.entity != Entity.Null)
            {
                PayRentPanelContext payRentPanelContext = new()
                {
                    Rent = SystemAPI.GetComponent<RentComponent>(clickedProperty.ValueRO.entity).Value,
                };
                SystemAPI.SetSingleton(new PayRentPanelContextComponent { Value = payRentPanelContext });
            }
        }
    }
}

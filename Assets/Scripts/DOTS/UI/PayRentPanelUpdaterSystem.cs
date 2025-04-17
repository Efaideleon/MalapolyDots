using Unity.Burst;
using Unity.Entities;

public struct PayRentPanelContextComponent : IComponentData
{
    public PayRentPanelContext Value;
}

[BurstCompile]
public partial struct PayRentPanelUpdaterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingleton(new PayRentPanelContextComponent { Value = default });
        state.RequireForUpdate<PayRentPanelContextComponent>();
        state.RequireForUpdate<LandedOnSpace>();
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

        foreach ( var landedOnSpace in 
                SystemAPI.Query<
                    RefRO<LandedOnSpace>
                >().
                WithChangeFilter<LandedOnSpace>())
        {
            var landedOnSpaceEntity = landedOnSpace.ValueRO.entity;
            if (landedOnSpaceEntity != Entity.Null && SystemAPI.HasComponent<PropertySpaceTag>(landedOnSpaceEntity))
            {
                PayRentPanelContext payRentPanelContext = new()
                {
                        Rent = SystemAPI.GetComponent<RentComponent>(landedOnSpaceEntity).Value,
                };
                SystemAPI.SetSingleton(new PayRentPanelContextComponent { Value = payRentPanelContext });
            }
        }
    }
}

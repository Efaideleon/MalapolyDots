using Unity.Burst;
using Unity.Entities;

public struct PurchasePropertyPanelContextComponent : IComponentData
{
    public PurchasePropertyPanelContext Value;
}

[BurstCompile]
public partial struct PurchasePropertyPanelUpdaterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingleton(new PurchasePropertyPanelContextComponent { Value = default });
        state.RequireForUpdate<PurchasePropertyPanelContextComponent>();
        state.RequireForUpdate<LastPropertyClicked>();
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
            var clickedPropertyEntity = clickedProperty.ValueRO.entity;
            if (clickedPropertyEntity != Entity.Null && SystemAPI.HasComponent<PropertySpaceTag>(clickedPropertyEntity))
            {
                PurchasePropertyPanelContext purchasePropertyPanelContext = new()
                {
                    spaceEntity = clickedProperty.ValueRO.entity,
                    entityManager = state.EntityManager,
                    playerID = SystemAPI.GetSingleton<CurrentPlayerID>().Value
                };
                SystemAPI.SetSingleton(new PurchasePropertyPanelContextComponent { Value = purchasePropertyPanelContext });
            }
        }
    }
}

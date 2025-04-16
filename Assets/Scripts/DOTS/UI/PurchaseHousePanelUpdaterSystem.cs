using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;

public struct PurhcaseHousePanelContextComponent : IComponentData
{
    public PurchaseHousePanelContext Value;
}

public partial struct PurchaseHousePanelUpdaterSystem : ISystem
{
    public void OnCreate(ref SystemState state) 
    {
        state.EntityManager.CreateSingleton(new PurhcaseHousePanelContextComponent { Value = default });
        state.RequireForUpdate<HouseCount>();
        state.RequireForUpdate<NameComponent>();
        state.RequireForUpdate<PropertySpaceTag>();
        state.RequireForUpdate<PanelControllers>();
        state.RequireForUpdate<PurhcaseHousePanelContextComponent>();
    }

    public void OnUpdate(ref SystemState state) 
    { 
        foreach (var (houseCount, name, _) in 
                SystemAPI.Query<
                    RefRO<HouseCount>,
                    RefRO<NameComponent>,
                    RefRO<PropertySpaceTag>
                >()
                .WithChangeFilter<HouseCount>())
        {
            var purchaseHousePanelContext = SystemAPI.GetSingletonRW<PurhcaseHousePanelContextComponent>();
            if (purchaseHousePanelContext.ValueRO.Value.Name == name.ValueRO.Value)
            {
                // BUG: Is this even possible or do we have to reassigned the component using SetComponent to trigger
                // the WithChangeFilter?
                purchaseHousePanelContext.ValueRW.Value.HousesOwned = houseCount.ValueRO.Value;
                continue;
            }
        }
    }
}

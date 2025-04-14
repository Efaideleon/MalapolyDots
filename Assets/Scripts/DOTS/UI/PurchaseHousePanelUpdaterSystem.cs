using Unity.Entities;

public partial struct PurchaseHousePanelUpdaterSystem : ISystem
{
    public void OnCreate(ref SystemState state) 
    {
        state.RequireForUpdate<HouseCount>();
        state.RequireForUpdate<NameComponent>();
        state.RequireForUpdate<PropertySpaceTag>();
        state.RequireForUpdate<PanelControllers>();
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
            var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();

            // On the first load, panelControllers is null 
            if (panelControllers.purchasePanelController != null)
            {
                var panel = panelControllers.purchasePanelController.PurchaseHousePanel;
                var context = panel.Context;
                if (context.Name == name.ValueRO.Value)
                {
                    context.HousesOwned = houseCount.ValueRO.Value;
                    panel.Context = context;
                    panel.Update();
                }
            }
        }
    }
}

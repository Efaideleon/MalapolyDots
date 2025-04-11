using Unity.Entities;

public partial struct BuyHousesUIUpdaterSystem : ISystem
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
            // On the first load, buyHouseUIController is null 
            if (panelControllers.buyHouseUIController != null)
            {
                foreach (var element in panelControllers.buyHouseUIController.BuyHouseUI.PropertyNameCounterElementsList)
                {
                    var context = element.Context;
                    if (context.Name == name.ValueRO.Value)
                    {
                        context.HousesOwned = houseCount.ValueRO.Value;
                        element.Context = context;
                        element.Update();
                    }
                }
            }
        }
    }
}

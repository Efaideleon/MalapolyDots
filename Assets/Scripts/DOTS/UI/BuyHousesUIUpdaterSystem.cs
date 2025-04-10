using Unity.Entities;

public partial struct BuyHousesUIUpdaterSystem : ISystem
{
    public void OnCreate(ref SystemState state) 
    {
        state.RequireForUpdate<HouseCount>();
        state.RequireForUpdate<NameComponent>();
        state.RequireForUpdate<PropertySpaceTag>();
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
            var uiPanels = SystemAPI.ManagedAPI.GetSingleton<OverLayPanels>();
            foreach (var propertyNameCounterElement in uiPanels.buyhouseUI.PropertyNameCounterElementsList)
            {
                var context = propertyNameCounterElement.Context;
                if (context.Name == name.ValueRO.Value)
                {
                    context.HousesOwned = houseCount.ValueRO.Value;
                    propertyNameCounterElement.Context = context;
                    propertyNameCounterElement.Update();
                }
            }
        }
    }
}

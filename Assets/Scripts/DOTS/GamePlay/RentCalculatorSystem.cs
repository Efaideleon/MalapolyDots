using Unity.Entities;

public partial struct RentCalculatorSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        // Get the base rent of each place
        // Use the algorithm to find the final current rent
        // set the final rent
        foreach (var (rent, owner, entity) in 
                SystemAPI.Query<
                    RefRW<RentComponent>, 
                    RefRO<OwnerComponent>
                >()
                .WithEntityAccess()
                .WithChangeFilter<OwnerComponent>())
        {
            var baseRentsBuffer = SystemAPI.GetBuffer<BaseRentBuffer>(entity);
            if (owner.ValueRO.ID != PropertyConstants.Vacant)
            {
                rent.ValueRW.Value = baseRentsBuffer[0].Value;
            }
        }
    }
}

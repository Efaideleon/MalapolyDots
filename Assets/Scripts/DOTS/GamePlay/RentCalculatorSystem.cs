using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct RentCalculatorSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
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

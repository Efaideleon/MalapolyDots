using DOTS.Constants;
using DOTS.DataComponents;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct RentCalculatorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<OwnerComponent>();
            state.RequireForUpdate<RentComponent>();
            state.RequireForUpdate<BaseRentBuffer>();
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
}

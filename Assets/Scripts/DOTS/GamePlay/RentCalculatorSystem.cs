using DOTS.Constants;
using DOTS.DataComponents;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct RentCalculatorSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<OwnerComponent>();
            state.RequireForUpdate<GhostRentComponent>();
            state.RequireForUpdate<BaseRentBuffer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (rent, owner, entity) in
                    SystemAPI.Query<
                    RefRW<GhostRentComponent>,
                    RefRO<OwnerComponent>
                    >()
                    .WithEntityAccess()
                    .WithChangeFilter<OwnerComponent>())
            {
                var baseRentsBuffer = SystemAPI.GetBuffer<BaseRentBuffer>(entity);
                if (owner.ValueRO.ID != PropertyConstants.Vacant)
                {
                    rent.ValueRW.Value = baseRentsBuffer[0].Value;
                    UnityEngine.Debug.Log($"[RentCalculatorSystem] | rent to charge: {rent.ValueRO.Value}");
                }
            }
        }
    }
}

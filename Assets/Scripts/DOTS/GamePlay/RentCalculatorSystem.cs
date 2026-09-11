using DOTS.Constants;
using DOTS.DataComponents;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(BuyHouseSystem))]
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
                    .WithAll<BaseRentBuffer>()
                    .WithEntityAccess())
            {
                var baseRentsBuffer = SystemAPI.GetBuffer<BaseRentBuffer>(entity);
                int houses = SystemAPI.HasComponent<HouseCount>(entity) ? SystemAPI.GetComponent<HouseCount>(entity).Value : 0;
                int rentIndex = Unity.Mathematics.math.clamp(houses, 0, baseRentsBuffer.Length - 1);
                int value = owner.ValueRO.ID != PropertyConstants.Vacant && baseRentsBuffer.Length > 0
                    ? baseRentsBuffer[rentIndex].Value : 0;
                var kind = SystemAPI.HasComponent<PropertyRentKindComponent>(entity)
                    ? SystemAPI.GetComponent<PropertyRentKindComponent>(entity).Value : PropertyRentKind.Street;
                if (owner.ValueRO.ID != PropertyConstants.Vacant && baseRentsBuffer.Length > 0)
                {
                    if (kind == PropertyRentKind.Street && houses == 0 &&
                        SystemAPI.HasComponent<MonopolyFlagComponent>(entity) && SystemAPI.GetComponent<MonopolyFlagComponent>(entity).Value)
                        value *= 2;
                    else if (kind != PropertyRentKind.Street)
                    {
                        int owned = 0;
                        foreach (var (otherOwner, otherKind) in SystemAPI.Query<RefRO<OwnerComponent>, RefRO<PropertyRentKindComponent>>())
                            if (otherOwner.ValueRO.ID == owner.ValueRO.ID && otherKind.ValueRO.Value == kind) owned++;
                        int index = Unity.Mathematics.math.clamp(owned - 1, 0, baseRentsBuffer.Length - 1);
                        value = baseRentsBuffer[index].Value;
                        if (kind == PropertyRentKind.Utility)
                            value *= SystemAPI.TryGetSingleton<RollAmountComponent>(out var roll) ? roll.Value : 0;
                    }
                }
                if (rent.ValueRO.Value != value) rent.ValueRW.Value = value;
            }
        }
    }
}

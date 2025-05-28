using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct InitializePropertyForSaleSignSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ForSaleComponent>();
            state.RequireForUpdate<ForSaleSignTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            
            foreach ( var (forSaleSign, _, entity) in SystemAPI.Query<RefRW<ForSaleComponent>, RefRO<PropertySpaceTag>>().WithEntityAccess())
            {
                var linkedEntityGroup = SystemAPI.GetBuffer<LinkedEntityGroup>(entity);

                foreach (var child in linkedEntityGroup)
                {
                    if (child.Value != entity)
                    {
                        if (SystemAPI.HasComponent<ForSaleSignTag>(child.Value))
                        {
                            forSaleSign.ValueRW.entity = child.Value;
                        }
                    }
                }
            }
        }
    }
}

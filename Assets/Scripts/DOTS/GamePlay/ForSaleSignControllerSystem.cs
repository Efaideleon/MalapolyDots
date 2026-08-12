using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Malapoly.DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

namespace DOTS.GamePlay
{
    // This System controls if a for sale sign for a property should be hidden or showing
    // based on if they have have an owner or not.
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct ForSaleSignControllerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ForSaleComponent>();
            state.RequireForUpdate<ForSaleSignTag>();
            state.RequireForUpdate<VisibleStateComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (owner, forSaleComponent, _) in 
                    SystemAPI.Query<
                        RefRO<OwnerComponent>,
                        RefRW<ForSaleComponent>,
                        RefRO<PropertySpaceTag>
                    >()
                    .WithChangeFilter<OwnerComponent>())
            {
                bool purchased = owner.ValueRO.ID != PropertyConstants.Vacant;

                if (purchased)
                {
                    // The ForSaleComponent points to the child forsale sign entity.
                    // Should i make it a ghost component?
                    // or there a method to reference child entity better now?
                    var forSaleSignEntity = forSaleComponent.ValueRW.entity;
                    ref var visibleStateCompRW = ref SystemAPI.GetComponentRW<VisibleStateComponent>(forSaleSignEntity).ValueRW;
                    visibleStateCompRW.Value = VisibleState.Hiding;
                }
            }
        }
    }
}

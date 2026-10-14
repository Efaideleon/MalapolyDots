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

                // The ForSaleComponent points to the child sign entity. Ownership
                // changes in both directions: hide the sign when bought and restore
                // it after the deed returns to the bank.
                var forSaleSignEntity = forSaleComponent.ValueRO.entity;
                if (forSaleSignEntity == Entity.Null ||
                    !SystemAPI.HasComponent<VisibleStateComponent>(forSaleSignEntity)) continue;

                ref var visibleState = ref SystemAPI.GetComponentRW<VisibleStateComponent>(forSaleSignEntity).ValueRW;
                visibleState.Value = purchased ? VisibleState.Hiding : VisibleState.Visible;
            }
        }
    }
}

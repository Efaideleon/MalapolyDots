using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.UI.Controllers;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.UI.Systems
{
    public struct PayRentPanelContextComponent : IComponentData
    {
        public PayRentPanelContext Value;
    }

    [BurstCompile]
    public partial struct PayRentPanelUpdaterSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new PayRentPanelContextComponent { Value = default });
            state.RequireForUpdate<PayRentPanelContextComponent>();
            state.RequireForUpdate<LandedOnSpace>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ( var clickedProperty in 
                    SystemAPI.Query<
                    RefRO<LastPropertyClicked>
                    >().
                    WithChangeFilter<LastPropertyClicked>())
            {
                if (clickedProperty.ValueRO.entity != Entity.Null)
                {
                    PayRentPanelContext payRentPanelContext = new()
                    {
                        Rent = SystemAPI.GetComponent<RentComponent>(clickedProperty.ValueRO.entity).Value,
                    };
                    var panelContext = SystemAPI.GetSingletonRW<PayRentPanelContextComponent>();
                    panelContext.ValueRW = new PayRentPanelContextComponent { Value = payRentPanelContext };
                }
            }

            foreach ( var landedOnSpace in 
                    SystemAPI.Query<
                    RefRO<LandedOnSpace>
                    >().
                    WithChangeFilter<LandedOnSpace>())
            {
                var landedOnSpaceEntity = landedOnSpace.ValueRO.entity;
                if (landedOnSpaceEntity != Entity.Null && SystemAPI.HasComponent<PropertySpaceTag>(landedOnSpaceEntity)) 
                {
                    PayRentPanelContext payRentPanelContext = new()
                    {
                        Rent = SystemAPI.GetComponent<RentComponent>(landedOnSpaceEntity).Value,
                    };
                    var panelContext = SystemAPI.GetSingletonRW<PayRentPanelContextComponent>();
                    panelContext.ValueRW = new PayRentPanelContextComponent { Value = payRentPanelContext };
                }
            }
        }
    }
}

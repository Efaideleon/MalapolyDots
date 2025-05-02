using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.UI.Panels;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.UI.Systems
{
    public struct PurhcaseHousePanelContextComponent : IComponentData
    {
        public PurchaseHousePanelContext Value;
    }

    [BurstCompile]
    public partial struct PurchaseHousePanelUpdaterSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state) 
        {
            state.EntityManager.CreateSingleton(new PurhcaseHousePanelContextComponent { Value = default });
            state.RequireForUpdate<HouseCount>();
            state.RequireForUpdate<NameComponent>();
            state.RequireForUpdate<PropertySpaceTag>();
            state.RequireForUpdate<PurhcaseHousePanelContextComponent>();
            state.RequireForUpdate<LastPropertyClicked>();
        }

        [BurstCompile]
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
                var purchaseHousePanelContext = SystemAPI.GetSingletonRW<PurhcaseHousePanelContextComponent>();
                if (purchaseHousePanelContext.ValueRO.Value.Name == name.ValueRO.Value)
                {
                    // BUG: Is this even possible or do we have to reassigned the component using SetComponent to trigger
                    // the WithChangeFilter?
                    purchaseHousePanelContext.ValueRW.Value.HousesOwned = houseCount.ValueRO.Value;
                    continue;
                }
            }

            foreach ( var clickedProperty in 
                    SystemAPI.Query<
                    RefRO<LastPropertyClicked>
                    >().
                    WithChangeFilter<LastPropertyClicked>())
            {
                var clickedPropertyEntity = clickedProperty.ValueRO.entity;
                if (clickedPropertyEntity != Entity.Null && SystemAPI.HasComponent<PropertySpaceTag>(clickedPropertyEntity))
                {
                    PurchaseHousePanelContext purchaseHouseContext = new()
                    {
                        Name = SystemAPI.GetComponent<NameComponent>(clickedPropertyEntity).Value,
                        HousesOwned = SystemAPI.GetComponent<HouseCount>(clickedPropertyEntity).Value,
                        Price = 10,
                    };
                    var panelContext = SystemAPI.GetSingletonRW<PurhcaseHousePanelContextComponent>();
                    panelContext.ValueRW = new PurhcaseHousePanelContextComponent { Value = purchaseHouseContext};
                }
            }
        }
    }
}

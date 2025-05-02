using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.UI.Controllers;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.UI.Systems
{
    public struct PurchasePropertyPanelContextComponent : IComponentData
    {
        public PurchasePropertyPanelContext Value;
    }

    [BurstCompile]
    public partial struct PurchasePropertyPanelUpdaterSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new PurchasePropertyPanelContextComponent { Value = default });
            state.RequireForUpdate<PurchasePropertyPanelContextComponent>();
            state.RequireForUpdate<LastPropertyClicked>();
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
                var clickedPropertyEntity = clickedProperty.ValueRO.entity;
                if (clickedPropertyEntity != Entity.Null && SystemAPI.HasComponent<PropertySpaceTag>(clickedPropertyEntity))
                {
                    PurchasePropertyPanelContext purchasePropertyPanelContext = new()
                    {
                        Name = SystemAPI.GetComponent<NameComponent>(clickedPropertyEntity).Value,
                        Price = SystemAPI.GetComponent<PriceComponent>(clickedPropertyEntity).Value
                    };
                    SystemAPI.SetSingleton(new PurchasePropertyPanelContextComponent { Value = purchasePropertyPanelContext });
                }
            }

            foreach (var onLandSpace in SystemAPI.Query<RefRO<LandedOnSpace>>().WithChangeFilter<LandedOnSpace>())
            {
                if (SystemAPI.HasComponent<PropertySpaceTag>(onLandSpace.ValueRO.entity))
                {
                    var onLandProperty = onLandSpace.ValueRO.entity;
                    if (onLandProperty != Entity.Null && SystemAPI.HasComponent<PropertySpaceTag>(onLandProperty))
                    {
                        PurchasePropertyPanelContext purchasePropertyPanelContext = new()
                        {
                            Name = SystemAPI.GetComponent<NameComponent>(onLandProperty).Value,
                            Price = SystemAPI.GetComponent<PriceComponent>(onLandProperty).Value
                        };
                        SystemAPI.SetSingleton(new PurchasePropertyPanelContextComponent { Value = purchasePropertyPanelContext });
                    }
                }
            }
        }
    }
}

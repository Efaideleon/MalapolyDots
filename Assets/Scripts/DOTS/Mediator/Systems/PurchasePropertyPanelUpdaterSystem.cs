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
            state.RequireForUpdate<PropertyEventComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach ( var property in 
                    SystemAPI.Query<
                    RefRO<PropertyEventComponent>
                    >().
                    WithChangeFilter<PropertyEventComponent>())
            {
                var propertyEntity = property.ValueRO.entity;
                if (propertyEntity != Entity.Null)
                {
                    PurchasePropertyPanelContext purchasePropertyPanelContext = new()
                    {
                        Name = SystemAPI.GetComponent<NameComponent>(propertyEntity).Value,
                        Price = SystemAPI.GetComponent<PriceComponent>(propertyEntity).Value
                    };
                    var panelContext = SystemAPI.GetSingletonRW<PurchasePropertyPanelContextComponent>();
                    panelContext.ValueRW = new PurchasePropertyPanelContextComponent { Value = purchasePropertyPanelContext };
                }
            }
        }
    }
}

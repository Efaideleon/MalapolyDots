using Unity.Entities;
using Unity.Entities.Graphics;

namespace DOTS.GamePlay
{
    public struct LastPropertyOutlined : IComponentData
    {
        public Entity Entity;
    }

    public partial struct OutlineSelectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ClickedPropertyComponent>();
            state.RequireForUpdate<LastPropertyOutlined>();
            state.EntityManager.CreateSingleton<LastPropertyOutlined>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var clickedProperty in
                    SystemAPI.Query<
                        RefRO<ClickedPropertyComponent>
                    >()
                    .WithChangeFilter<ClickedPropertyComponent>())
            { 
                var clickedEntity = clickedProperty.ValueRO.entity;

                var lastPropertyOutlined = SystemAPI.GetSingletonRW<LastPropertyOutlined>();

                // Disable the 'outline' around the property
                if (lastPropertyOutlined.ValueRO.Entity != Entity.Null && lastPropertyOutlined.ValueRO.Entity != clickedEntity)
                {
                    ToggleOutline(ref state, ref lastPropertyOutlined.ValueRW.Entity, 7);
                    lastPropertyOutlined.ValueRW.Entity = clickedEntity;
                }

                // Enable the 'outline' around the property.
                bool isAPropertyClicked = clickedEntity != Entity.Null;
                if (isAPropertyClicked)
                {
                    ToggleOutline(ref state, ref clickedEntity, 6);
                    lastPropertyOutlined.ValueRW.Entity = clickedEntity;
                }
            }
        }

        private readonly void ToggleOutline(
                ref SystemState state,
                ref Entity entityToModify,
                int layer
        )
        {
            var oldSettings = state.EntityManager.GetSharedComponentManaged<RenderFilterSettings>(entityToModify);
            var newSettings = oldSettings;
            newSettings.Layer = layer;

            var ecb = GetECB(ref state);
            ecb.SetSharedComponentManaged(entityToModify, newSettings);
        }

        private readonly EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
         }
    }
}

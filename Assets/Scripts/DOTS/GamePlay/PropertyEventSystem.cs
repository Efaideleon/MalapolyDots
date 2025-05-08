using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public struct LastPropertyClicked : IComponentData
    {
        public Entity entity;
    }

    [BurstCompile]
    public partial struct PropertyEventSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new PropertyEventComponent { entity = Entity.Null });
            state.RequireForUpdate<LastPropertyClicked>();
            state.RequireForUpdate<PropertySpaceTag>();
            state.RequireForUpdate<LandedOnSpace>();
            state.EntityManager.CreateSingleton(new LastPropertyClicked { entity = Entity.Null });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var lastPropertyClicked in SystemAPI.Query<RefRO<LastPropertyClicked>>().WithChangeFilter<LastPropertyClicked>())
            {
                if (lastPropertyClicked.ValueRO.entity != Entity.Null && 
                        SystemAPI.HasComponent<PropertySpaceTag>(lastPropertyClicked.ValueRO.entity))
                {
                    var propertyEvent = SystemAPI.GetSingletonRW<PropertyEventComponent>();
                    propertyEvent.ValueRW.entity = lastPropertyClicked.ValueRO.entity;
                }
            }

            foreach (var onLandSpace in SystemAPI.Query<RefRO<LandedOnSpace>>().WithChangeFilter<LandedOnSpace>())
            {
                var onLandEntity = onLandSpace.ValueRO.entity;
                if (onLandEntity != Entity.Null && SystemAPI.HasComponent<PropertySpaceTag>(onLandEntity))
                {
                    var propertyEvent = SystemAPI.GetSingletonRW<PropertyEventComponent>();
                    propertyEvent.ValueRW.entity = onLandEntity;
                }
            }
        }
    }
}

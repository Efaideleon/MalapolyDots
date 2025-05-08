using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

namespace DOTS.GamePlay
{
    public struct ClickedPropertyComponent : IComponentData
    {
        public Entity entity;
    }

    [BurstCompile]
    public partial struct PropertyClickSystem : ISystem
    {
        public void OnCreate(ref SystemState state) 
        {
            state.EntityManager.CreateSingleton<ClickedPropertyComponent>();
            state.RequireForUpdate<ClickRayCastData>();
            state.RequireForUpdate<ClickedPropertyComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) 
        {
            PhysicsWorld world = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            var collisionWorld = world.CollisionWorld;

            // doesn't handle double tapping the same stop
            foreach (var clickRayCastData in SystemAPI.Query<RefRO<ClickRayCastData>>().WithChangeFilter<ClickRayCastData>())
            {
                RaycastInput input = new()
                {
                    Start = clickRayCastData.ValueRO.RayOrigin,
                    End = clickRayCastData.ValueRO.RayEnd,
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ~0u,
                        CollidesWith = ~0u,
                        GroupIndex = 0
                    }
                };
                collisionWorld.CastRay(input, out RaycastHit hit);
                var clickedProperty = SystemAPI.GetSingletonRW<ClickedPropertyComponent>();
                clickedProperty.ValueRW = new ClickedPropertyComponent { entity = hit.Entity };
            }
        }
    }
}

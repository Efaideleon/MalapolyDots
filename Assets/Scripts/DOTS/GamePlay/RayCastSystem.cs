using Unity.Burst; 
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine.InputSystem;

namespace DOTS.GamePlay
{
    public struct HitData
    {
        public Entity Entity;
        public float3 Position;
    }

    public struct RayCastResult : IComponentData
    {
        public HitData FloorHit;
        public HitData PropertyHit;
        public InputActionPhase ClickPhase;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct RayCastSystem : ISystem
    {
        const uint floorLayerBitMask = 1u << 8;
        const uint ignoreFloorLayerBitMask = ~(1u << 8);

        public void OnCreate(ref SystemState state)
        { 
            state.RequireForUpdate<ClickData>();
            state.EntityManager.CreateSingleton(new RayCastResult { FloorHit = default, PropertyHit = default, ClickPhase = default });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // When a click is detected we fire the ray cast.
            foreach (var clickData in SystemAPI.Query<RefRO<ClickData>>().WithChangeFilter<ClickData>())
            {
                PhysicsWorld world = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
                var collisionWorld = world.CollisionWorld;

                var ray = SystemAPI.GetSingleton<ClickRayCastData>();

                RaycastInput floorRayInput = CreateRaycastInput(ray.RayOrigin, ray.RayEnd, floorLayerBitMask);
                collisionWorld.CastRay(floorRayInput, out RaycastHit floorHit);

                RaycastInput propertyRayInput = CreateRaycastInput(ray.RayOrigin, ray.RayEnd, ignoreFloorLayerBitMask);
                collisionWorld.CastRay(propertyRayInput, out RaycastHit propertyHit);

                ref var result = ref SystemAPI.GetSingletonRW<RayCastResult>().ValueRW;
                result.FloorHit = new HitData { Entity = floorHit.Entity, Position = floorHit.Position };
                result.PropertyHit = new HitData { Entity = propertyHit.Entity, Position = propertyHit.Position };
                result.ClickPhase = clickData.ValueRO.Phase;
            }
        }

        [BurstCompile]
        public readonly RaycastInput CreateRaycastInput(float3 origin, float3 end, uint collisionBitMask)
        {
            RaycastInput input = new()
            {
                Start = origin,
                End = end,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = collisionBitMask,
                    GroupIndex = 0
                }
            };

            return input;
        }
    }
}

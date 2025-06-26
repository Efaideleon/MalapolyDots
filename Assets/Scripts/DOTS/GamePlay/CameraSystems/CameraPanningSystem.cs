// ========================================================================
// **Camera Panning System**
// * Pans the camera by dragging finger on floor.
// ========================================================================
using DOTS.GamePlay.CameraSystems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

using RaycastHit = Unity.Physics.RaycastHit;

namespace DOTS.GamePlay
{
    public struct Cache : IComponentData
    {
        public float3 Momentum;
        public InputActionPhase ClickPhase;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct CameraPanningSystem : ISystem
    {
        const uint floorLayerBitMask = 1u << 8;
        const float MaxFlingSpeed = 30f;
        const float DampingPerSecond = 5f;
        const uint StartPanningThreshold = 0;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<MainCameraTranslateData>();
            state.RequireForUpdate<DeltaClickRayCastData>();
            state.RequireForUpdate<ClickRayCastData>();
            state.RequireForUpdate<ClickData>();
            state.RequireForUpdate<RayCastResult>();
            state.EntityManager.CreateSingleton(new Cache { Momentum = default, ClickPhase = default });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorld world = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            var collisionWorld = world.CollisionWorld;

            ref var camTranslateData = ref SystemAPI.GetSingletonRW<MainCameraTranslateData>().ValueRW;
            ref var cache = ref SystemAPI.GetSingletonRW<Cache>().ValueRW;
            var dt = SystemAPI.Time.DeltaTime;

            foreach (var hit in SystemAPI.Query<RefRO<RayCastResult>>().WithChangeFilter<RayCastResult>())
            {
                // Check if we can pan the camera.
                bool isUIButtonClicked = SystemAPI.GetSingleton<UIButtonDirtyFlag>().Value;
                bool isFreeCamera = SystemAPI.GetSingleton<FreeCameraToggleFlag>().Value;
                bool isPropertyClicked = hit.ValueRO.PropertyHit.Entity != Entity.Null;

                if (!CanPan(isUIButtonClicked, isFreeCamera, isPropertyClicked))
                    break;

                var rayBefore = SystemAPI.GetSingleton<DeltaClickRayCastData>();
                RaycastInput rayBeforeInput = CreateRaycastInput(rayBefore.RayOrigin, rayBefore.RayEnd, floorLayerBitMask);

                HitData floorHitNow = hit.ValueRO.FloorHit;
                collisionWorld.CastRay(rayBeforeInput, out RaycastHit floorHitBefore);

                bool isDraggingFloor = floorHitNow.Entity != Entity.Null && floorHitBefore.Entity != Entity.Null;

                switch (hit.ValueRO.ClickPhase)
                {
                    case InputActionPhase.Performed:
                        if (isDraggingFloor)
                        {
                            var delta = floorHitBefore.Position - floorHitNow.Position;

                            if (math.lengthsq(delta) > StartPanningThreshold)
                            {
                                camTranslateData.Delta = delta;
                                CacheMomentum(ref cache, delta, dt);
                            }
                            else
                                camTranslateData.Delta = 0;

                            cache.ClickPhase = InputActionPhase.Performed;
                        }
                        break;
                    case InputActionPhase.Started:
                        camTranslateData.Delta = 0;
                        cache.Momentum = 0;
                        cache.ClickPhase = InputActionPhase.Started;
                        break;
                    case InputActionPhase.Canceled:
                        cache.ClickPhase = InputActionPhase.Canceled;
                        break;
                }
            }

            if (math.lengthsq(cache.Momentum) > 0 && cache.ClickPhase == InputActionPhase.Canceled)
                ApplyMomentum(ref cache, ref camTranslateData, in dt);
        }

        [BurstCompile]
        public readonly void ApplyMomentum(ref Cache cache, ref MainCameraTranslateData camTD, in float dt)
        {
            var move = cache.Momentum * dt;

            // Updating camera translation data.
            camTD.Delta = move;

            // Damping momentum.
            cache.Momentum = new float3(math.lerp(cache.Momentum, float3.zero, DampingPerSecond * dt));

            // Resetting momentum.
            if (math.lengthsq(cache.Momentum) < 0.0002f)
                cache.Momentum = float3.zero;
        }

        [BurstCompile]
        public readonly void CacheMomentum(ref Cache cache, in float3 delta, in float dt)
        {
            var velocity = delta / dt;
            var speed = math.length(velocity);

            var direction = velocity / speed; 

            if (speed > MaxFlingSpeed)
                velocity = direction * MaxFlingSpeed;

            cache.Momentum = velocity;
        }

        [BurstCompile]
        public readonly bool CanPan(bool isUIButtonClicked, bool isFreeCamera, bool isPropertyClicked)
        {
                if (isUIButtonClicked)
                {
#if UNITY_EDITOR
                    Debug.Log("[CameraPanningSystem] | UI Element or Button Clicked. No Panning.");
#endif
                    return false;
                }
                if (isPropertyClicked)
                {
#if UNITY_EDITOR
                    Debug.Log("[CameraPanningSystem] | Entity Hit. No Panning.");
#endif
                    return false;
                }
                if (!isFreeCamera)
                {
#if UNITY_EDITOR
                    Debug.Log("[CameraPanningSystem] | FreeCamera is off. No Panning.");
#endif
                    return false;
                }
                return true;
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

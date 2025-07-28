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
    public struct CameraPanState : IComponentData
    {
        public float3 Momentum;
        public InputActionPhase ClickPhase;
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateBefore(typeof(CameraManageUpdateSystem))]
    [BurstCompile]
    public partial struct CameraPanningSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<MainCameraTranslateData>();
            state.RequireForUpdate<DeltaClickRayCastData>();
            state.RequireForUpdate<ClickRayCastData>();
            state.RequireForUpdate<ClickData>();
            state.RequireForUpdate<RayCastResult>();
            state.RequireForUpdate<CameraPanSettings>();
            state.EntityManager.CreateSingleton(new CameraPanState { Momentum = default, ClickPhase = default });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            bool isFreeCamera = SystemAPI.GetSingleton<FreeCameraToggleFlag>().Value;
            if (!isFreeCamera) return;

            PhysicsWorld world = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            var collisionWorld = world.CollisionWorld;

            ref var cache = ref SystemAPI.GetSingletonRW<CameraPanState>().ValueRW;
            var dt = SystemAPI.Time.DeltaTime;
            var settings = SystemAPI.GetSingleton<CameraPanSettings>();

            SystemAPI.GetSingletonRW<MainCameraTranslateData>().ValueRW.Delta = 0;

            foreach (var hit in SystemAPI.Query<RefRO<RayCastResult>>().WithChangeFilter<RayCastResult>())
            {
                var camTranslateData = SystemAPI.GetSingletonRW<MainCameraTranslateData>();
                // Check if we can pan the camera.
                bool isUIButtonClicked = SystemAPI.GetSingleton<UIButtonDirtyFlag>().Value;
                bool isPropertyClicked = hit.ValueRO.PropertyHit.Entity != Entity.Null;

                if (!CanPan(isUIButtonClicked, isPropertyClicked))
                    break;

                var rayBefore = SystemAPI.GetSingleton<DeltaClickRayCastData>();
                RaycastInput rayBeforeInput = CreateRaycastInput(rayBefore.RayOrigin, rayBefore.RayEnd, settings.FloorLayerBitMask);

                HitData floorHitNow = hit.ValueRO.FloorHit;

                collisionWorld.CastRay(rayBeforeInput, out RaycastHit floorHitBefore);

                bool isTouchingFloor = floorHitNow.Entity != Entity.Null && floorHitBefore.Entity != Entity.Null;

                //Debug.Log($"[CameraPanningSystem] | Processing Raycast");
                switch (hit.ValueRO.ClickPhase)
                {
                    case InputActionPhase.Performed:
                        cache.Momentum = 0;
                        cache.ClickPhase = InputActionPhase.Performed;
                        if (isTouchingFloor)
                        {
                            var delta = floorHitBefore.Position - floorHitNow.Position;

                            if (math.lengthsq(delta) > settings.StartPanningThreshold)
                            {
                                camTranslateData.ValueRW.Delta = delta;
                                //Debug.Log($"[CameraPanningSystem] | Panning. Delta: {delta}");
                                CacheMomentum(ref cache, delta, dt, settings.MaxFlingSpeed);
                            }
                        }
                        break;
                    case InputActionPhase.Started:
                        cache.Momentum = 0;
                        cache.ClickPhase = InputActionPhase.Started;
                        camTranslateData.ValueRW.Delta = 0;
                        break;
                    case InputActionPhase.Canceled:
                        cache.ClickPhase = InputActionPhase.Canceled;
                        camTranslateData.ValueRW.Delta = 0;
                        break;
                }
            }

            var isCameraStill = math.lengthsq(SystemAPI.GetSingleton<MainCameraTranslateData>().Delta) == 0;
            var isClickHeld = cache.ClickPhase == InputActionPhase.Performed;

            if (isClickHeld && isCameraStill)
            {
                cache.Momentum = 0;
            }

            if (math.lengthsq(cache.Momentum) > 0 && cache.ClickPhase == InputActionPhase.Canceled)
            {
                //Debug.Log($"[CameraPanningSystem] | Applying momentum: {math.lengthsq(cache.Momentum)}");
                var camTranslateData = SystemAPI.GetSingletonRW<MainCameraTranslateData>();
                ApplyMomentum(ref cache, ref camTranslateData.ValueRW, in dt, settings.DampingPerSecond);
            }
        }

        [BurstCompile]
        public readonly void ApplyMomentum(ref CameraPanState cache, ref MainCameraTranslateData camTD, in float dt, float dampingPerSecond)
        {
            var move = cache.Momentum * dt;

            // Updating camera translation data.
            camTD.Delta = move;

            // Damping momentum.
            cache.Momentum -= dampingPerSecond * dt * cache.Momentum;

            // Resetting momentum.
            if (math.lengthsq(cache.Momentum) < 0.0002f)
                cache.Momentum = 0;
        }

        [BurstCompile]
        public readonly void CacheMomentum(ref CameraPanState cache, in float3 delta, in float dt, float maxFlingSpeed)
        {
            var velocity = delta / dt;
            var speed = math.length(velocity);

            var direction = velocity / speed;

            if (speed > maxFlingSpeed)
                velocity = direction * maxFlingSpeed;

            cache.Momentum = velocity;
        }

        [BurstCompile]
        public readonly bool CanPan(bool isUIButtonClicked, bool isPropertyClicked)
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

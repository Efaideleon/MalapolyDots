// ========================================================================
// **Camera Panning System**
// * Pans the camera by dragging finger on floor.
// ========================================================================
using DOTS.EventBuses;
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
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct CameraPanningSystem : ISystem
    {
        const uint floorLayerBitMask = 1u << 8;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<MainCameraTranslateData>();
            state.RequireForUpdate<DeltaClickRayCastData>();
            state.RequireForUpdate<ClickRayCastData>();
            state.RequireForUpdate<ClickData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorld world = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
            var collisionWorld = world.CollisionWorld;

            var camTranslateData = SystemAPI.GetSingletonRW<MainCameraTranslateData>();

            foreach (var clickData in SystemAPI.Query<RefRO<ClickData>>().WithChangeFilter<ClickData>())
            {
                var isUIButtonClicked = SystemAPI.GetSingleton<UIButtonDirtyFlag>().Value;
                UnityEngine.Debug.Log($"CameraPanningSystem isUIButtonClicked: {isUIButtonClicked}");
                if (isUIButtonClicked)
                {
                    UnityEngine.Debug.Log("CameraPanningSystem raycast skipped");
                    break;
                }

                var rayNow = SystemAPI.GetSingleton<ClickRayCastData>();
                var rayBefore = SystemAPI.GetSingleton<DeltaClickRayCastData>();

                RaycastInput rayNowInput = CreateRaycastInput(rayNow.RayOrigin, rayNow.RayEnd, floorLayerBitMask);
                RaycastInput rayBeforeInput = CreateRaycastInput(rayBefore.RayOrigin, rayBefore.RayEnd, floorLayerBitMask); 

                collisionWorld.CastRay(rayNowInput, out RaycastHit rayHitNow);
                collisionWorld.CastRay(rayBeforeInput, out RaycastHit rayHitBefore);

                switch (clickData.ValueRO.Phase)
                {
                    case InputActionPhase.Performed:
                        if (rayHitNow.Entity != Entity.Null && rayHitBefore.Entity != Entity.Null)
                        {
                            var planePositionDelta = rayHitBefore.Position - rayHitNow.Position;
                            camTranslateData.ValueRW.Delta = planePositionDelta;
                            camTranslateData.ValueRW.Space = Space.World;
                        }
                        break;
                }
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

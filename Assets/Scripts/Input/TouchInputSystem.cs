using Assets.Scripts.DOTS.Characters;
using DOTS.DataComponents;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Input
{
#nullable enable
    public struct IsTouchingUIElement : IComponentData
    {
        public bool Value;
    }

    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial struct TouchInputSystem : ISystem, ISystemStartStop
    {
        private const float RayLength = 1000f;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<InputActionsComponent>();
            state.RequireForUpdate<ClickData>();
            state.RequireForUpdate<ClickRayCastData>();
            state.RequireForUpdate<DeltaClickRayCastData>();
            state.RequireForUpdate<CurrentCameraManagedObject>();
            state.RequireForUpdate<NetworkId>();
            state.EntityManager.CreateSingleton(new IsTouchingUIElement { Value = false });
        }

        public void OnStartRunning(ref SystemState state)
        {
            var inputActions = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
            inputActions.Value.Touch.Enable();
        }

        public void OnUpdate(ref SystemState state)
        {
            var inputActions = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
            var touchAction = inputActions.Value.Touch;
            var pressAction = touchAction.Press;
            var positionAction = touchAction.Position;
            var deltaPositionAction = touchAction.DeltaPosition;

            // Send ecs event that touch pressed started.
            foreach (var touchStarted in SystemAPI.Query<RefRW<TouchStartedInput>>().WithAll<GhostOwnerIsLocal>())
            {
                touchStarted.ValueRW.IsTapped = default;
                if (pressAction.WasPressedThisFrame())
                {
                    touchStarted.ValueRW.IsTapped.Set();
                }
            }

            // Send ecs event that touch pressed was canceled.
            foreach (var touchCanceled in SystemAPI.Query<RefRW<TouchCanceledInput>>().WithAll<GhostOwnerIsLocal>())
            {
                touchCanceled.ValueRW.IsTapped = default;
                if (pressAction.WasCompletedThisFrame())
                {
                    touchCanceled.ValueRW.IsTapped.Set();
                }
            }

            // Read the positon where we are tapping.
            var position = touchAction.Position.ReadValue<Vector2>();
            foreach (var (touchRayCastData, touchPosition) in SystemAPI.Query<RefRW<TouchRayCastDataInput>, RefRW<TouchPositionInput>>().WithAll<GhostOwnerIsLocal>())
            {
                if (positionAction.WasPerformedThisFrame())
                {
                    var camera = SystemAPI.ManagedAPI.GetSingleton<CurrentCameraManagedObject>();
                    if (camera.Camera != null)
                    {
                        var rayData = InputHelperMethods.GetRayData(position, camera.Camera);
                        touchRayCastData.ValueRW.RayOrigin = rayData.origin;
                        touchRayCastData.ValueRW.RayDirection = rayData.direction;
                        touchRayCastData.ValueRW.RayEnd = rayData.origin + (rayData.direction * RayLength);
                        touchPosition.ValueRW.Position = position;
                        touchPosition.ValueRW.IsHeld.Set();
                    }
                }
                else
                {
                    touchPosition.ValueRW.IsHeld = default;
                }
            }

            // TODO: Panning.
            // if (deltaPositionAction.triggered)
            // {
            //     var currentCamera = SystemAPI.ManagedAPI.GetSingleton<CurrentCameraManagedObject>();
            //     if (currentCamera.Camera != null)
            //     {
            //         var deltaPosition = inputActions.Value.Touch.DeltaPosition.ReadValue<Vector2>();
            //         var rayData = InputHelperMethods.GetRayBeforeData(position, deltaPosition, currentCamera.Camera);
            //         ref var deltaRayCastData = ref SystemAPI.GetSingletonRW<DeltaClickRayCastData>().ValueRW;
            //
            //         InputHelperMethods.SetDeltaRayCastData(ref deltaRayCastData, RayLength, rayData);
            //     }
            // }
        }

        public void OnStopRunning(ref SystemState state)
        {
            var InputActions = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
            InputActions.Value.Touch.Disable();
        }
    }
}

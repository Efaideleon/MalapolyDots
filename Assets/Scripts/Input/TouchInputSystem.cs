using Assets.Scripts.DOTS.Characters;
using DOTS.DataComponents;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Input
{
#nullable enable
    public struct LocalPropertyTap : IComponentData
    {
        public uint Sequence;
        public bool BlockedByUI;
        public Unity.Mathematics.float3 RayOrigin;
        public Unity.Mathematics.float3 RayEnd;
    }

    public struct IsTouchingUIElement : IComponentData
    {
        public bool Value;
    }

    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    public partial struct TouchInputSystem : ISystem, ISystemStartStop
    {
        private const float RayLength = 1000f;
        private int lastTapFrame;
        private bool pressBeganOnUI;
        public void OnCreate(ref SystemState state)
        {
            lastTapFrame = -1;
            state.RequireForUpdate<InputActionsComponent>();
            state.RequireForUpdate<ClickData>();
            state.RequireForUpdate<ClickRayCastData>();
            state.RequireForUpdate<DeltaClickRayCastData>();
            state.RequireForUpdate<NetworkId>();
            state.EntityManager.CreateSingleton(new IsTouchingUIElement { Value = false });
            state.EntityManager.CreateSingleton<LocalPropertyTap>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var inputActions = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
            var touch = inputActions.Value.Touch;
            touch.Disable();
            touch.Press.ApplyBindingOverride(0, "<Touchscreen>/primaryTouch/press");
            if (!HasBinding(touch.Press, "<Mouse>/leftButton"))
                touch.Press.AddBinding("<Mouse>/leftButton");
            if (!HasBinding(touch.Position, "<Mouse>/position"))
                touch.Position.AddBinding("<Mouse>/position");
            inputActions.Value.Touch.Enable();
        }

        private static bool HasBinding(InputAction action, string path)
        {
            foreach (var binding in action.bindings)
                if (binding.effectivePath == path) return true;
            return false;
        }

        public void OnUpdate(ref SystemState state)
        {
            var inputActions = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
            var touchAction = inputActions.Value.Touch;
            var pressAction = touchAction.Press;
            var positionAction = touchAction.Position;
            var deltaPositionAction = touchAction.DeltaPosition;
            var camera = Camera.main;
            // Mouse movement must not overwrite simulated/real touch coordinates.
            var source = pressAction.activeControl?.device;
            var position = source is Touchscreen touchscreen
                ? touchscreen.primaryTouch.position.ReadValue()
                : source is Pointer pointer
                    ? pointer.position.ReadValue()
                    : positionAction.ReadValue<Vector2>();
            if (pressAction.WasPressedThisFrame())
            {
                pressBeganOnUI = IsOverUI(position);
                SystemAPI.GetSingletonRW<IsTouchingUIElement>().ValueRW.Value = pressBeganOnUI;
                if (camera == null)
                    Debug.LogWarning("[PropertyTap] Tap received, but no MainCamera is available for raycasting.");
                else
                    Debug.Log($"[PropertyTap] Tap received at {position}, device={source?.displayName}, camera={camera.name}.");
                if (camera != null && lastTapFrame != Time.frameCount)
                {
                    lastTapFrame = Time.frameCount;
                    var ray = camera.ScreenPointToRay(position);
                    ref var tap = ref SystemAPI.GetSingletonRW<LocalPropertyTap>().ValueRW;
                    tap.Sequence = tap.Sequence == uint.MaxValue ? 1u : tap.Sequence + 1u;
                    tap.BlockedByUI = pressBeganOnUI;
                    tap.RayOrigin = ray.origin;
                    tap.RayEnd = ray.origin + ray.direction * RayLength;
                }
            }

            // Send ecs event that touch pressed started.
            foreach (var touchStarted in SystemAPI.Query<RefRW<TouchStartedInput>>().WithAll<GhostOwnerIsLocal>())
            {
                touchStarted.ValueRW.IsTapped = default;
                if (pressAction.WasPressedThisFrame() && !pressBeganOnUI)
                {
                    touchStarted.ValueRW.IsTapped.Set();
                }
            }

            // Send ecs event that touch pressed was canceled.
            foreach (var touchCanceled in SystemAPI.Query<RefRW<TouchCanceledInput>>().WithAll<GhostOwnerIsLocal>())
            {
                touchCanceled.ValueRW.IsTapped = default;
                if (pressAction.WasCompletedThisFrame() && !pressBeganOnUI)
                {
                    touchCanceled.ValueRW.IsTapped.Set();
                }
            }

            // Read the positon where we are tapping.
            foreach (var (touchRayCastData, touchPosition) in SystemAPI.Query<RefRW<TouchRayCastDataInput>, RefRW<TouchPositionInput>>().WithAll<GhostOwnerIsLocal>())
            {
                touchPosition.ValueRW.IsHeld = default;
                if (pressAction.WasPressedThisFrame() || positionAction.WasPerformedThisFrame())
                {
                    if (camera != null)
                    {
                        var rayData = InputHelperMethods.GetRayData(position, camera);
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

        private static bool IsOverUI(Vector2 screenPosition)
        {
            // UI Toolkit uses top-left screen coordinates; Input System uses bottom-left.
            var uiPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);
            foreach (var document in Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
            {
                var root = document.rootVisualElement;
                if (!document.isActiveAndEnabled || root?.panel == null) continue;
                var picked = root.panel.Pick(RuntimePanelUtils.ScreenToPanel(root.panel, uiPosition));
                if (picked != null && picked != root && root.Contains(picked)) return true;
            }
            return false;
        }
    }
}

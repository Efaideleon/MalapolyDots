using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine;

#nullable enable
public struct IsTouchingUIElement : IComponentData
{
    public bool Value;
}

[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial struct TouchInputSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InputActionsComponent>();
        state.RequireForUpdate<ClickData>();
        state.RequireForUpdate<ClickRayCastData>();
        state.RequireForUpdate<DeltaClickRayCastData>();
        state.RequireForUpdate<CurrentCameraManagedObject>();
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

        var position = inputActions.Value.Touch.Position.ReadValue<Vector2>();
        if (touchAction.Press.WasPerformedThisFrame() || touchAction.Position.WasPerformedThisFrame())
        {
            float rayLength = 1000f;
            var currentCamera = SystemAPI.ManagedAPI.GetSingleton<CurrentCameraManagedObject>();
            if (currentCamera.Camera != null)
            {
                var rayData = InputHelperMethods.GetRayData(position, currentCamera.Camera);
                ref var clickData = ref SystemAPI.GetSingletonRW<ClickData>().ValueRW;
                ref var rayCastData = ref SystemAPI.GetSingletonRW<ClickRayCastData>().ValueRW;

                InputHelperMethods.SetClickData(ref clickData, position, touchAction.Press.phase);
                InputHelperMethods.SetRayCastData(ref rayCastData, rayData, rayLength);
            }
        }
        if (touchAction.DeltaPosition.WasPerformedThisFrame())
        {
            float rayLength = 1000f;
            var currentCamera = SystemAPI.ManagedAPI.GetSingleton<CurrentCameraManagedObject>();
            if (currentCamera.Camera != null)
            {
                var deltaPosition = inputActions.Value.Touch.DeltaPosition.ReadValue<Vector2>();
                var rayData = InputHelperMethods.GetRayBeforeData(position, deltaPosition, currentCamera.Camera);
                ref var deltaRayCastData = ref SystemAPI.GetSingletonRW<DeltaClickRayCastData>().ValueRW;

                InputHelperMethods.SetDeltaRayCastData(ref deltaRayCastData, rayLength, rayData);
            }
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
        var InputActions = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
        InputActions.Value.Touch.Disable();
    }
}

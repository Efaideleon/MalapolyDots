using System;
using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

#nullable enable
public class TouchCallBack : IComponentData
{
    public Action<InputAction.CallbackContext> TouchPositionCallback;
}

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
        state.RequireForUpdate<CurrentCameraMangedObject>();
        state.EntityManager.CreateSingleton(new TouchCallBack { TouchPositionCallback = null });
        state.EntityManager.CreateSingleton(new IsTouchingUIElement { Value = false });
    }

    public void OnStartRunning(ref SystemState state)
    {
        var InputActions = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
        var clickDataQuery = SystemAPI.QueryBuilder().WithAllRW<ClickData>().Build();
        var clickRayCastDataQuery = SystemAPI.QueryBuilder().WithAllRW<ClickRayCastData>().Build();
        var deltaRayCastDataQuery = SystemAPI.QueryBuilder().WithAllRW<DeltaClickRayCastData>().Build();
        var touchCallback = SystemAPI.ManagedAPI.GetSingleton<TouchCallBack>();
        var currentCamera = SystemAPI.ManagedAPI.GetSingleton<CurrentCameraMangedObject>();
        InputActions.Value.Touch.Enable();

        float rayLength = 1000f;

        touchCallback.TouchPositionCallback = ctx =>
        {
            var position = InputActions.Value.Touch.Position.ReadValue<Vector2>();
            if (ctx.action == InputActions.Value.Touch.Press || ctx.action == InputActions.Value.Touch.Position)
            {
                if (currentCamera.Camera != null)
                {
                    var rayData = InputHelperMethods.GetRayData(position, currentCamera.Camera);
                    ref var clickData = ref clickDataQuery.GetSingletonRW<ClickData>().ValueRW;
                    ref var rayCastData = ref clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW;

                    InputHelperMethods.SetClickData(ref clickData, position, ctx.phase);
                    InputHelperMethods.SetRayCastData(ref rayCastData, rayData, rayLength);
                }
            }
            else if (ctx.action == InputActions.Value.Touch.DeltaPosition)
            {
                if (currentCamera.Camera != null)
                {
                    var deltaPosition = InputActions.Value.Touch.DeltaPosition.ReadValue<Vector2>();
                    var rayData = InputHelperMethods.GetRayBeforeData(position, deltaPosition, currentCamera.Camera);
                    ref var deltaRayCastData = ref deltaRayCastDataQuery.GetSingletonRW<DeltaClickRayCastData>().ValueRW;

                    InputHelperMethods.SetDeltaRayCastData(ref deltaRayCastData, rayLength, rayData);
                }
            }
        };

        InputActions.Value.Touch.Press.started += touchCallback.TouchPositionCallback;
        InputActions.Value.Touch.Press.canceled += touchCallback.TouchPositionCallback;
        InputActions.Value.Touch.Position.performed += touchCallback.TouchPositionCallback;
        InputActions.Value.Touch.DeltaPosition.performed += touchCallback.TouchPositionCallback;
    }

    public void OnUpdate(ref SystemState state)
    { }

    public void OnStopRunning(ref SystemState state)
    {
        var InputActions = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
        var touchCallback = SystemAPI.ManagedAPI.GetSingleton<TouchCallBack>();
        InputActions.Value.Touch.Press.started -= touchCallback.TouchPositionCallback;
        InputActions.Value.Touch.Press.canceled -= touchCallback.TouchPositionCallback;
        InputActions.Value.Touch.Position.performed -= touchCallback.TouchPositionCallback;
        InputActions.Value.Touch.DeltaPosition.performed -= touchCallback.TouchPositionCallback;
        InputActions.Value.Touch.Disable();
    }
}

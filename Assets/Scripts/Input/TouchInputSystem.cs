using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchCallBack : IComponentData
{
    public Action<InputAction.CallbackContext> TouchPositionCallback;
}

[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial struct TouchInputSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InputActionsComponent>();
        state.RequireForUpdate<ClickData>();
        state.RequireForUpdate<ClickRayCastData>();
        state.EntityManager.CreateSingleton(new TouchCallBack { TouchPositionCallback = null });
    }

    public void OnStartRunning(ref SystemState state)
    {
        var InputActions = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
        var clickDataQuery = SystemAPI.QueryBuilder().WithAllRW<ClickData>().Build();
        var clickRayCastDataQuery = SystemAPI.QueryBuilder().WithAllRW<ClickRayCastData>().Build();
        var touchCallback = SystemAPI.ManagedAPI.GetSingleton<TouchCallBack>();
        InputActions.Value.Touch.Enable();
        float rayLenght = 1000f;

        touchCallback.TouchPositionCallback = ctx => 
        {
            switch (ctx.phase)
            {
                case InputActionPhase.Started:
                    var position = InputActions.Value.Touch.Position.ReadValue<Vector2>();
                    var (clickPosition, rayData) = InputHelperMethods.GetClickPositionAndRay(position);
                    InputHelperMethods.SetClickData(
                            ref clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW,
                            ref clickDataQuery.GetSingletonRW<ClickData>().ValueRW,
                            ctx.phase,
                            rayLenght,
                            clickPosition,
                            rayData);
                    break;
                case InputActionPhase.Canceled:
                    var position2 = InputActions.Value.Touch.Position.ReadValue<Vector2>();
                    var (clickPosition2, rayData2) = InputHelperMethods.GetClickPositionAndRay(position2);
                    InputHelperMethods.SetClickData ( 
                            ref clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW,
                            ref clickDataQuery.GetSingletonRW<ClickData>().ValueRW,
                            ctx.phase,
                            rayLenght,
                            clickPosition2,
                            rayData2);
                    break;

            }
        };

        InputActions.Value.Touch.Press.started += touchCallback.TouchPositionCallback;
        InputActions.Value.Touch.Press.canceled += touchCallback.TouchPositionCallback;

    }

    public void OnUpdate(ref SystemState state)
    {}

    public void OnStopRunning(ref SystemState state)
    { 
        var InputActions = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
        var touchCallback = SystemAPI.ManagedAPI.GetSingleton<TouchCallBack>();
        InputActions.Value.Touch.Press.started -= touchCallback.TouchPositionCallback;
        InputActions.Value.Touch.Press.canceled -= touchCallback.TouchPositionCallback;
    }
}

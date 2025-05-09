using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputActionsComponent : IComponentData
{
    public InputActions Value;
}

public struct ClickData : IComponentData
{
    public float2 Position;
    public InputActionPhase Phase;
}

public struct ClickRayCastData : IComponentData
{
    public float3 RayOrigin;
    public float3 RayDirection;
    public float3 RayEnd;
}

public class ClickCallback : IComponentData
{
    public Action<InputAction.CallbackContext> leftClickCallback;
}

public struct RayData
{
    public float3 origin;
    public float3 direction;
}

[UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
public partial struct UIInputSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        // ClickRayCastData Entity
        var clickRayCastDataEntity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<ClickRayCastData>()
        });
        SystemAPI.SetComponent(clickRayCastDataEntity, new ClickRayCastData
        {
            RayOrigin = default, 
            RayDirection = default,
            RayEnd = default 
        });

        // ClickData Entity
        var clickDataEntity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<ClickData>()
        });
        SystemAPI.SetComponent(clickDataEntity, new ClickData
        {
            Position = new float2(),
            Phase = default 
        });

        // InputAction Entity
        var entity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentObject(entity, new InputActionsComponent
        {
            Value = new InputActions()
        });

        // ClickCalllback Entity
        var clickCallbackEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentObject(clickCallbackEntity, new ClickCallback
        {
            leftClickCallback = null
        });
        state.RequireForUpdate<ClickCallback>();
        state.RequireForUpdate<InputActionsComponent>();
        state.RequireForUpdate<ClickData>();
        state.RequireForUpdate<ClickRayCastData>();
    }

    public void OnStartRunning(ref SystemState state)
    {
        var inputActionsComponent = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
        
        var clickCallback = SystemAPI.ManagedAPI.GetSingleton<ClickCallback>();

        var clickDataQuery = SystemAPI.QueryBuilder().WithAllRW<ClickData>().Build();
        var clickRayCastDataQuery = SystemAPI.QueryBuilder().WithAllRW<ClickRayCastData>().Build();
        float rayLenght = 1000f;
        clickCallback.leftClickCallback = ctx => 
        {
            switch(ctx.phase)
            {
                case InputActionPhase.Started:
                    var (clickPosition, rayData) = GetClickPositionAndRay();
                    SetClickData(
                            ref clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW,
                            ref clickDataQuery.GetSingletonRW<ClickData>().ValueRW,
                            ctx.phase,
                            rayLenght,
                            clickPosition,
                            rayData);
                    break;
                case InputActionPhase.Canceled:
                    var (clickPosition2, rayData2) = GetClickPositionAndRay();
                    SetClickData ( 
                            ref clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW,
                            ref clickDataQuery.GetSingletonRW<ClickData>().ValueRW,
                            ctx.phase,
                            rayLenght,
                            clickPosition2,
                            rayData2);
                    break;
            }
        };
        inputActionsComponent.Value.Mouse.LeftClick.started += clickCallback.leftClickCallback;
        inputActionsComponent.Value.Mouse.LeftClick.canceled += clickCallback.leftClickCallback;
        inputActionsComponent.Value.Enable();
    }

    private static (float2, RayData) GetClickPositionAndRay()
    {
        var clickPositionVector = Mouse.current.position.ReadValue();
        float2 clickPositionFloat2 = new(clickPositionVector.x, clickPositionVector.y);
        Ray ray = Camera.main.ScreenPointToRay(clickPositionVector);
        RayData rayData = new() { origin = ray.origin, direction = ray.direction };
        return (clickPositionFloat2, rayData);
    }

    [BurstCompile]
    private static void SetClickData(
            ref ClickRayCastData clickRayCastData,
            ref ClickData clickData,
            InputActionPhase phase,
            float rayLenght,
            float2 clickPosition,
            RayData rayData)
    {
        clickRayCastData.RayOrigin = rayData.origin;
        clickRayCastData.RayDirection = rayData.direction;
        clickRayCastData.RayEnd = rayData.origin + (rayData.direction * rayLenght);
        clickData.Position = clickPosition;  
        clickData.Phase = phase;  
    }

    public void OnUpdate(ref SystemState state) { }

    public void OnStopRunning(ref SystemState state)
    {
        var inputActionsComponent = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
        var clickCallback = SystemAPI.ManagedAPI.GetSingleton<ClickCallback>();
        inputActionsComponent.Value.Mouse.LeftClick.started -= clickCallback.leftClickCallback;
        inputActionsComponent.Value.Mouse.LeftClick.canceled -= clickCallback.leftClickCallback;
        inputActionsComponent.Value.Disable();
    }
}

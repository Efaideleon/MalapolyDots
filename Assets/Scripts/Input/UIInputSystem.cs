using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
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
        UnityEngine.Debug.Log("UIInputSystem OnStartRunning getting called");
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
                    var clickPositionVector = Mouse.current.position.ReadValue();
                    UnityEngine.Debug.Log("Mouse button pressed");
                    float2 clickPositionFloat2 = new(clickPositionVector.x, clickPositionVector.y);
                    Ray ray = Camera.main.ScreenPointToRay(clickPositionVector);
                    clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW.RayOrigin = ray.origin;  
                    clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW.RayDirection = ray.direction;  
                    clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW.RayEnd = ray.origin + (ray.direction * rayLenght);  
                    clickDataQuery.GetSingletonRW<ClickData>().ValueRW.Position = clickPositionFloat2;  
                    clickDataQuery.GetSingletonRW<ClickData>().ValueRW.Phase = InputActionPhase.Started;  
                    break;
                case InputActionPhase.Canceled:
                    UnityEngine.Debug.Log("Mouse button released");
                    clickPositionVector = Mouse.current.position.ReadValue();
                    clickPositionFloat2 = new(clickPositionVector.x, clickPositionVector.y);
                    ray = Camera.main.ScreenPointToRay(clickPositionVector);
                    clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW.RayOrigin = ray.origin;  
                    clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW.RayDirection = ray.direction;  
                    clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW.RayEnd = ray.origin + (ray.direction * rayLenght);  
                    clickDataQuery.GetSingletonRW<ClickData>().ValueRW.Position = clickPositionFloat2;  
                    clickDataQuery.GetSingletonRW<ClickData>().ValueRW.Phase = InputActionPhase.Canceled;  
                    break;
            }
        };
        inputActionsComponent.Value.Mouse.LeftClick.started += clickCallback.leftClickCallback;
        inputActionsComponent.Value.Mouse.LeftClick.canceled += clickCallback.leftClickCallback;
        inputActionsComponent.Value.Enable();
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

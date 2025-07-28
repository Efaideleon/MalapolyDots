using System;
using Unity.Entities;
using Unity.Mathematics;
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

public struct DeltaClickRayCastData : IComponentData
{
    public float3 RayOrigin;
    public float3 RayDirection;
    public float3 RayEnd;


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

        // DeltaClickRayCastData
        var deltaRayCastDataEntity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<DeltaClickRayCastData>()
        });
        SystemAPI.SetComponent(deltaRayCastDataEntity, new DeltaClickRayCastData
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
        // var inputActionsComponent = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
        //
        // var clickCallback = SystemAPI.ManagedAPI.GetSingleton<ClickCallback>();
        //
        // var clickDataQuery = SystemAPI.QueryBuilder().WithAllRW<ClickData>().Build();
        // var clickRayCastDataQuery = SystemAPI.QueryBuilder().WithAllRW<ClickRayCastData>().Build();
        // float rayLenght = 1000f;
        // clickCallback.leftClickCallback = ctx => 
        // {
        //     switch(ctx.phase)
        //     {
        //         case InputActionPhase.Started:
        //             var (clickPosition, rayData) = InputHelperMethods.GetClickPositionAndRay();
        //             InputHelperMethods.SetClickData(
        //                     ref clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW,
        //                     ref clickDataQuery.GetSingletonRW<ClickData>().ValueRW,
        //                     ctx.phase,
        //                     rayLenght,
        //                     clickPosition,
        //                     rayData);
        //             break;
        //         case InputActionPhase.Canceled:
        //             var (clickPosition2, rayData2) = InputHelperMethods.GetClickPositionAndRay();
        //             InputHelperMethods.SetClickData ( 
        //                     ref clickRayCastDataQuery.GetSingletonRW<ClickRayCastData>().ValueRW,
        //                     ref clickDataQuery.GetSingletonRW<ClickData>().ValueRW,
        //                     ctx.phase,
        //                     rayLenght,
        //                     clickPosition2,
        //                     rayData2);
        //             break;
        //     }
        // };
        // inputActionsComponent.Value.Mouse.LeftClick.started += clickCallback.leftClickCallback;
        // inputActionsComponent.Value.Mouse.LeftClick.canceled += clickCallback.leftClickCallback;
        // inputActionsComponent.Value.Enable();
    }


    public void OnUpdate(ref SystemState state) { }

    public void OnStopRunning(ref SystemState state)
    {
        // var inputActionsComponent = SystemAPI.ManagedAPI.GetSingleton<InputActionsComponent>();
        // var clickCallback = SystemAPI.ManagedAPI.GetSingleton<ClickCallback>();
        // inputActionsComponent.Value.Mouse.LeftClick.started -= clickCallback.leftClickCallback;
        // inputActionsComponent.Value.Mouse.LeftClick.canceled -= clickCallback.leftClickCallback;
        // inputActionsComponent.Value.Disable();
    }
}

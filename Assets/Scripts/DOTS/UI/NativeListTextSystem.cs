using Unity.Burst;
using Unity.Entities;
using Unity.Collections;

public struct SomeEvent
{
    public SpaceTypeEnum EventType;
}

public struct SomeEvents : IComponentData
{
    public NativeQueue<SomeEvent> EventQueue;
}

public struct SomeTestIndex : IComponentData
{
    public int Index;
}

[BurstCompile]
public partial struct NativeListTextSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var currentPlayerIndexEntity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<SomeTestIndex>()
        });

        SystemAPI.SetComponent(currentPlayerIndexEntity, new SomeTestIndex
        {
            Index = 0
        });
        EntityQuery query = state.GetEntityQuery(ComponentType.ReadOnly<SomeEvents>());
        if (query.IsEmpty)
        {
            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<SomeEvents>()
            });
            SystemAPI.SetComponent(entity, new SomeEvents
            {
                EventQueue = new NativeQueue<SomeEvent>(Allocator.Persistent)
            });
            UnityEngine.Debug.Log($"Creating SomeEventQueue");
        }
        else 
        {
            var entity = query.GetSingletonEntity();
            var events = state.EntityManager.GetComponentData<SomeEvents>(entity);

            if (events.EventQueue.IsCreated)
            {
                events.EventQueue.Dispose();
                events.EventQueue = new NativeQueue<SomeEvent>(Allocator.Persistent);
                state.EntityManager.SetComponentData(entity, events);
                UnityEngine.Debug.Log($"Reinitializing NativeQueue");
            }
        }

        state.RequireForUpdate<GameDataComponent>();
        state.RequireForUpdate<CurrPlayerID>();
        state.RequireForUpdate<SomeEvents>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    { }

    public void OnDestroy(ref SystemState state)
    {
        // Then free the SomeEvents.EventQueue since no anonymous function has a handle to the NativeQueue.
        var query = state.GetEntityQuery(ComponentType.ReadOnly<SomeEvents>());
        foreach (var entity in query.ToEntityArray(Allocator.Temp))
        {
            var someEvent = state.EntityManager.GetComponentData<SomeEvents>(entity);
            UnityEngine.Debug.Log($"someEvent lenght: {someEvent.EventQueue.Count} ");
            UnityEngine.Debug.Log($"someEvent exists : {someEvent.EventQueue.IsCreated}");
            if (someEvent.EventQueue.IsCreated)
            {
                UnityEngine.Debug.Log("Diposing SomeEvent NativeQueue");
                someEvent.EventQueue.Dispose();
                someEvent.EventQueue = default;
                state.EntityManager.SetComponentData(entity, someEvent);
            }
        }
    }
}

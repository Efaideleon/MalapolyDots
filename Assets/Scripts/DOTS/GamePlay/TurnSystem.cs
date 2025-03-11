using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

public struct TurnRequestEvent
{}

public struct TurnEvents : IComponentData
{
    public NativeQueue<TurnRequestEvent> EventQueue;
}

public struct CurrentPlayerIndex : IComponentData
{
    public int Index;
}

[BurstCompile]
public partial struct TurnSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        EntityQuery query = state.GetEntityQuery(ComponentType.ReadOnly<TurnEvents>());
        if (query.IsEmpty)
        {
            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<TurnEvents>()
            });
            SystemAPI.SetComponent(entity, new TurnEvents
            {
                EventQueue = new NativeQueue<TurnRequestEvent>(Allocator.Persistent) //Remeber to dispose
            });
        }
        var currentPlayerIndexEntity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<CurrentPlayerIndex>()
        });

        SystemAPI.SetComponent(currentPlayerIndexEntity, new CurrentPlayerIndex
        {
            Index = 0
        });

        state.RequireForUpdate<GameDataComponent>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(var turnManager in SystemAPI.Query<RefRW<TurnEvents>>().WithChangeFilter<TurnEvents>())
        {
            var characterSelectedNames = SystemAPI.GetSingletonBuffer<CharacterSelectedBuffer>();
            while(turnManager.ValueRW.EventQueue.TryDequeue(out var _))
            {
                Profiler.BeginSample("Profile for Setting next character turn");
                // Find the player with the current turn and set it to false
                foreach (var turnComponent in SystemAPI.Query<RefRW<PlayerTurnComponent>>())
                {
                    if (turnComponent.ValueRO.IsActive == true)
                        turnComponent.ValueRW.IsActive = false;
                }
                // Find the player with the next turn and set it to true
                foreach (var currentPlayerIndex in SystemAPI.Query<RefRW<CurrentPlayerIndex>>())
                {
                    currentPlayerIndex.ValueRW.Index = (currentPlayerIndex.ValueRW.Index + 1) % characterSelectedNames.Length;
                    foreach(var(nameComponent, turnComponent) in SystemAPI.Query<RefRO<NameDataComponent>, RefRW<PlayerTurnComponent>>())
                    {
                        if (characterSelectedNames[currentPlayerIndex.ValueRW.Index].Value == nameComponent.ValueRO.Value)
                        {
                            turnComponent.ValueRW.IsActive = true;
                        }
                    }
                }
                Profiler.EndSample();
                Debug.Break();
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        foreach(var turnManager in SystemAPI.Query<RefRW<TurnEvents>>())
        {
            turnManager.ValueRW.EventQueue.Clear();
            turnManager.ValueRW.EventQueue.Dispose();
        }
    }
}

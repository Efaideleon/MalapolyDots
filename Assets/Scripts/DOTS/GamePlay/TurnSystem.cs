using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

public struct TurnRequestEvent
{ }

public struct TurnEvents : IComponentData
{
    public NativeQueue<TurnRequestEvent> EventQueue;
}

public struct CharacterNameIndex : IComponentData
{
    public int Index;
}

// Should run after spawning
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
            ComponentType.ReadOnly<CharacterNameIndex>()
        });

        SystemAPI.SetComponent(currentPlayerIndexEntity, new CharacterNameIndex
        {
            Index = 0
        });

        state.RequireForUpdate<GameDataComponent>();
        state.RequireForUpdate<CurrPlayerID>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var turnManager in SystemAPI.Query<RefRW<TurnEvents>>().WithChangeFilter<TurnEvents>())
        {
            var characterSelectedNames = SystemAPI.GetSingletonBuffer<CharacterSelectedBuffer>();
            // Handle each change turn request
            //
            while (turnManager.ValueRW.EventQueue.TryDequeue(out var _))
            {
                // Find the player with the next turn and set it to true
                var currentPlayerIndex = SystemAPI.GetSingletonRW<CharacterNameIndex>();
                var nextPlayerIndex = (currentPlayerIndex.ValueRW.Index + 1) % characterSelectedNames.Length;
                currentPlayerIndex.ValueRW.Index = nextPlayerIndex;

                foreach (var (nameComponent, playerID) in SystemAPI.Query<RefRO<NameComponent>, RefRO<PlayerID>>())
                {
                    if (characterSelectedNames[currentPlayerIndex.ValueRO.Index].Value == nameComponent.ValueRO.Value)
                    {
                        var currentPlayerID = SystemAPI.GetSingletonRW<CurrPlayerID>();

                        currentPlayerID.ValueRW.Value = playerID.ValueRO.Value;
                    }
                }
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        foreach (var turnManager in SystemAPI.Query<RefRW<TurnEvents>>())
        {
            turnManager.ValueRW.EventQueue.Clear();
            turnManager.ValueRW.EventQueue.Dispose();
        }
    }
}

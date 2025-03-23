using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

public struct CharacterNameIndex : IComponentData
{
    public int Index;
}

public struct TransactionEvent
{
    public SpaceTypeEnum EventType;
}

public struct TransactionEvents : IComponentData
{
    public NativeQueue<TransactionEvent> EventQueue;
}

[BurstCompile]
public partial struct TransactionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var currentPlayerIndexEntity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<CharacterNameIndex>()
        });

        SystemAPI.SetComponent(currentPlayerIndexEntity, new CharacterNameIndex
        {
            Index = 0
        });
        EntityQuery query = state.GetEntityQuery(ComponentType.ReadOnly<TransactionEvents>());
        if (query.IsEmpty)
        {
            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<TransactionEvents>()
            });
            SystemAPI.SetComponent(entity, new TransactionEvents
            {
                EventQueue = new NativeQueue<TransactionEvent>(Allocator.Persistent)
            });
            UnityEngine.Debug.Log($"Creating NativeQueue");
        }
        else 
        {
            var entity = query.GetSingletonEntity();
            var events = state.EntityManager.GetComponentData<TransactionEvents>(entity);

            if (events.EventQueue.IsCreated)
            {
                events.EventQueue.Dispose();
                events.EventQueue = new NativeQueue<TransactionEvent>(Allocator.Persistent);
                state.EntityManager.SetComponentData(entity, events);
                UnityEngine.Debug.Log($"Reinitializing NativeQueue");
            }
        }

        state.RequireForUpdate<GameDataComponent>();
        state.RequireForUpdate<CurrPlayerID>();
        state.RequireForUpdate<TransactionEvents>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // foreach (var transactionEvents in SystemAPI.Query<RefRW<TransactionEvents>>().WithChangeFilter<TransactionEvents>())
        // {
        //     var characterSelectedNames = SystemAPI.GetSingletonBuffer<CharacterSelectedBuffer>();
        //     while (transactionEvents.ValueRW.EventQueue.TryDequeue(out var transactionEvent))
        //     {
        //         // Purchase the property if possible
        //         if (transactionEvent.EventType == SpaceTypeEnum.Property)
        //         {
        //             foreach (var (playerID, playerMoney) in SystemAPI.Query<RefRO<PlayerID>, RefRW<MoneyComponent>>())
        //             {
        //                 var currentPlayerID = SystemAPI.GetSingleton<CurrPlayerID>();
        //                 if (playerID.ValueRO.Value == currentPlayerID.Value)
        //                 {
        //                     var property = SystemAPI.GetSingleton<LandedOnSpace>();
        //                     var propertyPrice = SystemAPI.GetComponent<SpacePriceComponent>(property.entity);
        //                     playerMoney.ValueRW.Value -= propertyPrice.Value;
        //                 }
        //             }
        //         }
        //         // Handle each change turn request
        //         var currentPlayerIndex = SystemAPI.GetSingletonRW<CharacterNameIndex>();
        //         var nextPlayerIndex = (currentPlayerIndex.ValueRW.Index + 1) % characterSelectedNames.Length;
        //         currentPlayerIndex.ValueRW.Index = nextPlayerIndex;
        //
        //         foreach (var (nameComponent, playerID) in SystemAPI.Query<RefRO<NameComponent>, RefRO<PlayerID>>())
        //         {
        //             if (characterSelectedNames[currentPlayerIndex.ValueRO.Index].Value == nameComponent.ValueRO.Value)
        //             {
        //                 var currentPlayerID = SystemAPI.GetSingletonRW<CurrPlayerID>();
        //                 currentPlayerID.ValueRW.Value = playerID.ValueRO.Value;
        //             }
        //         }
        //     }
        // }
    }

    public void OnDestroy(ref SystemState state)
    { 
        // Then free the TransactionEvents.EventQueue since no anonymous function has a handle to the NativeQueue.
        var query = state.GetEntityQuery(ComponentType.ReadOnly<TransactionEvents>());
        foreach (var entity in query.ToEntityArray(Allocator.Temp))
        {
            var transactionEvent = state.EntityManager.GetComponentData<TransactionEvents>(entity);
            UnityEngine.Debug.Log($"transactionEvent lenght: {transactionEvent.EventQueue.Count} ");
            UnityEngine.Debug.Log($"transactionEvent exists : {transactionEvent.EventQueue.IsCreated}");
            if (transactionEvent.EventQueue.IsCreated)
            {
                UnityEngine.Debug.Log("Diposing NativeQueue");
                transactionEvent.EventQueue.Dispose();
                transactionEvent.EventQueue = default;
                state.EntityManager.SetComponentData(entity, transactionEvent);
            }
        }
    }
}

using Unity.Burst;
using Unity.Entities;

public struct CharacterNameIndex : IComponentData
{
    public int Index;
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

        state.RequireForUpdate<GameDataComponent>();
        state.RequireForUpdate<CurrPlayerID>();
        state.RequireForUpdate<TransactionEvents>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var transactionEvents in SystemAPI.Query<RefRW<TransactionEvents>>().WithChangeFilter<TransactionEvents>())
        {
            var characterSelectedNames = SystemAPI.GetSingletonBuffer<CharacterSelectedBuffer>();
            while (transactionEvents.ValueRW.EventQueue.TryDequeue(out var transactionEvent))
            {
                // Purchase the property if possible
                if (transactionEvent.EventType == TransactionEventsEnum.Purchase)
                {
                    foreach (var (playerID, playerMoney) in SystemAPI.Query<RefRO<PlayerID>, RefRW<MoneyComponent>>())
                    {
                        var currentPlayerID = SystemAPI.GetSingleton<CurrPlayerID>();
                        if (playerID.ValueRO.Value == currentPlayerID.Value)
                        {
                            var property = SystemAPI.GetSingleton<LandedOnSpace>();
                            var price = SystemAPI.GetComponent<SpacePriceComponent>(property.entity);
                            playerMoney.ValueRW.Value -= price.Value;
                            var owner = SystemAPI.GetComponentRW<OwnerComponent>(property.entity);
                            owner.ValueRW.OwnerID = playerID.ValueRO.Value;
                        }
                    }
                }

                if (transactionEvent.EventType == TransactionEventsEnum.ChangeTurn)
                {
                    // Handle each change turn request
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
    }
}

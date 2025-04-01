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
        state.RequireForUpdate<CurrentPlayerID>();
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
                if (transactionEvent.EventType == TransactionEventsEnum.PayRent)
                {
                    foreach (var (playerID, playerMoney) in SystemAPI.Query<RefRO<PlayerID>, RefRW<MoneyComponent>>())
                    {
                        var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();
                        if (playerID.ValueRO.Value == currentPlayerID.Value)
                        {
                            // Get the rent to pay
                            var property = SystemAPI.GetSingleton<LandedOnSpace>();
                            int rent = SystemAPI.GetComponent<RentComponent>(property.entity).Value;

                            // Charge the rent from the player paying
                            playerMoney.ValueRW.Value -= rent;

                            // Add the money from the rent to the right owner
                            int ownerID = SystemAPI.GetComponentRO<OwnerComponent>(property.entity).ValueRO.ID;
                            foreach (var (otherPlayerMoney, otherPlayerID) in 
                                    SystemAPI.Query<
                                        RefRW<MoneyComponent>,
                                        RefRO<PlayerID>
                                    >())
                            {
                                if (ownerID == otherPlayerID.ValueRO.Value)
                                {
                                    otherPlayerMoney.ValueRW.Value += rent;
                                }
                            }
                        }
                    }
                }

                // Purchase the property if possible
                if (transactionEvent.EventType == TransactionEventsEnum.Purchase)
                {
                    foreach (var (playerID, playerMoney) in SystemAPI.Query<RefRO<PlayerID>, RefRW<MoneyComponent>>())
                    {
                        var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();
                        if (playerID.ValueRO.Value == currentPlayerID.Value)
                        {
                            var property = SystemAPI.GetSingleton<LandedOnSpace>();
                            var price = SystemAPI.GetComponent<PriceComponent>(property.entity);
                            playerMoney.ValueRW.Value -= price.Value;
                            var owner = SystemAPI.GetComponentRW<OwnerComponent>(property.entity);
                            owner.ValueRW.ID = playerID.ValueRO.Value;
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
                            var currentPlayerID = SystemAPI.GetSingletonRW<CurrentPlayerID>();
                            currentPlayerID.ValueRW.Value = playerID.ValueRO.Value;
                        }
                    }
                }
            }
        }
    }
}

using DOTS.Characters;
using DOTS.DataComponents;
using DOTS.EventBuses;
using DOTS.GameData;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public struct CharacterNameIndex : IComponentData
    {
        public int Index;
    }

    public struct ChangeTurnBufferEvent : IBufferElementData
    {}

    public struct CurrentRound : IComponentData
    {
        public int Value;
    }

    [BurstCompile]
    public partial struct TransactionSystem : ISystem
    {
        private int currentTurn;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            currentTurn = 0;
            state.EntityManager.CreateSingleton(new CharacterNameIndex { Index = 0 });
            state.EntityManager.CreateSingletonBuffer<ChangeTurnBufferEvent>();
            state.EntityManager.CreateSingleton(new CurrentRound { Value = 0 });

            state.RequireForUpdate<GameDataComponent>();
            state.RequireForUpdate<CurrentPlayerID>();
            state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<TransactionEventBuffer>();
            state.RequireForUpdate<ClickedPropertyComponent>();
            state.RequireForUpdate<CharacterSelectedNameBuffer>();
            state.RequireForUpdate<BackDropEventBus>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var transactionBuffer in SystemAPI.Query<DynamicBuffer<TransactionEventBuffer>>().WithChangeFilter<TransactionEventBuffer>())
            {
                if (transactionBuffer.Length < 1)
                    continue;

                var characterSelectedNames = SystemAPI.GetSingletonBuffer<CharacterSelectedNameBuffer>();
                foreach (var transaction in transactionBuffer)
                {
                    if (transaction.EventType == TransactionEventType.PayRent)
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
                    if (transaction.EventType == TransactionEventType.Purchase)
                    {
                        foreach (var (playerID, playerMoney) in SystemAPI.Query<RefRO<PlayerID>, RefRW<MoneyComponent>>())
                        {
                            var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();
                            if (playerID.ValueRO.Value == currentPlayerID.Value)
                            {
                                var property = SystemAPI.GetSingleton<PropertyEventComponent>();
                                // TODO: Check if the player is on the this entity to be able to buy it
                                if (property.entity != Entity.Null && 
                                        SystemAPI.HasComponent<PropertySpaceTag>(property.entity))
                                {
                                    var price = SystemAPI.GetComponent<PriceComponent>(property.entity);
                                    playerMoney.ValueRW.Value -= price.Value;
                                    var owner = SystemAPI.GetComponentRW<OwnerComponent>(property.entity);
                                    owner.ValueRW.ID = playerID.ValueRO.Value;
                                }
                            }
                        }
                    }

                    if (transaction.EventType == TransactionEventType.ChangeTurn)
                    {
                        // Handle each change turn request
                        var totalRounds = SystemAPI.GetSingleton<LoginData>().NumberOfRounds;
                        var totalNumOfPlayer = SystemAPI.GetSingleton<LoginData>().NumberOfPlayers;
                        currentTurn += 1;
                        UnityEngine.Debug.Log($"Turn: {currentTurn}");
                        if (currentTurn == totalNumOfPlayer)
                        {
                            SystemAPI.GetSingletonRW<CurrentRound>().ValueRW.Value += 1;
                            currentTurn = 0;
                            UnityEngine.Debug.Log($"[TransactionSystem] | Changing Round {SystemAPI.GetSingleton<CurrentRound>().Value}");
                        }

                        var currentPlayerIndex = SystemAPI.GetSingletonRW<CharacterNameIndex>();
                        var nextPlayerIndex = (currentPlayerIndex.ValueRW.Index + 1) % characterSelectedNames.Length;
                        currentPlayerIndex.ValueRW.Index = nextPlayerIndex;

                        foreach (var (nameComponent, playerID, entity) in 
                                SystemAPI.Query<
                                RefRO<NameComponent>,
                                RefRO<PlayerID>
                                >()
                                .WithEntityAccess())
                        {
                            if (characterSelectedNames[currentPlayerIndex.ValueRO.Index].Name == nameComponent.ValueRO.Value)
                            {
                                SystemAPI.GetSingletonRW<CurrentPlayerID>().ValueRW.Value = playerID.ValueRO.Value;
                                SystemAPI.GetSingletonBuffer<ChangeTurnBufferEvent>().Add(new ChangeTurnBufferEvent{});
                                SystemAPI.GetSingletonRW<CurrentPlayerComponent>().ValueRW.entity = entity;
                            }
                        }
                        
                        // TODO: Move this to another system
                        var eventBuffer = SystemAPI.GetSingletonBuffer<BackDropEventBus>();
                        eventBuffer.Add(new BackDropEventBus{ });
                    }

                    if (transaction.EventType == TransactionEventType.PayTaxes)
                    {
                        foreach (var (playerID, playerMoney) in SystemAPI.Query<RefRO<PlayerID>, RefRW<MoneyComponent>>())
                        {
                            var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();
                            if (playerID.ValueRO.Value == currentPlayerID.Value)
                            {
                                var space = SystemAPI.GetSingleton<LandedOnSpace>();
                                if (space.entity != Entity.Null && 
                                        SystemAPI.HasComponent<TaxSpaceTag>(space.entity))
                                {
                                    // TODO: this value should come from a component in the tax
                                    var tax = 100_000; 
                                    playerMoney.ValueRW.Value -= tax;
                                }
                            }
                        }
                    }
                }

                transactionBuffer.Clear();
            }
        }
    }
}

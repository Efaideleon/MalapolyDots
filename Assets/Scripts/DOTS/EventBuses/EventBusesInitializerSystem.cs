using Unity.Entities; 
using Unity.Collections;

namespace DOTS.EventBuses
{
    public struct BuyHouseEventBuffer : IBufferElementData
    {
        // TODO: Change to a property Entity
        // The property is the name of the property to buy a house  
        public FixedString64Bytes property; // rename to propertyName
    }

    public struct RollEventBuffer : IBufferElementData { }

    public enum TransactionEventType
    {
        Purchase,
        ChangeTurn,
        PayRent,
        UpgradeHouse,
        Default
    }
    public struct TransactionEventBuffer : IBufferElementData
    {
        public TransactionEventType EventType;
    }

    public partial struct EventBusesInitializerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Initializing Event communication to buy houses
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<BuyHouseEventBuffer>(entity);

            // Initializing Roll Event Bus
            var rollBusBufferEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<RollEventBuffer>(rollBusBufferEntity);

            // Initializing Roll Event Bus
            var transactionEventEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<TransactionEventBuffer>(transactionEventEntity);

        }

        public void OnUpdate(ref SystemState state)
        {}
    }
}

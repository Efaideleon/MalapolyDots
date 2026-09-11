using Unity.Entities; 
using Unity.Collections;

namespace DOTS.EventBuses
{
    public struct BuyHouseEventBuffer : IBufferElementData
    {
        public int PropertyId;
        public int Count;
    }

    public struct RollEventBuffer : IBufferElementData { }

    public struct PurchasePropertyEventBuffer : IBufferElementData { }

    public enum TransactionEventType
    {
        Purchase,
        ChangeTurn,
        PayRent,
        UpgradeHouse,
        PayTaxes,
        Chance,
        Treasure,
        Go,
        Parking,
        GoToJail,
        Jail,
        Default
    }
    public struct TransactionEventBuffer : IBufferElementData
    {
        public TransactionEventType EventType;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
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

            var purchasePropertyEventBus = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<PurchasePropertyEventBuffer>(purchasePropertyEventBus);
        }

        public void OnUpdate(ref SystemState state)
        {}
    }
}

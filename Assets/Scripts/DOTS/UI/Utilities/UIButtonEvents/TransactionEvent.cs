using DOTS.EventBuses;
using Unity.Entities;

namespace DOTS.UI.Utilities.UIButtonEvents
{
    public class TransactionEvent : IButtonEvent
    {
        private EntityQuery _transactionEventBusQuery;
        private readonly TransactionEventType _eventType;

        public TransactionEvent(EntityQuery query, TransactionEventType eventType)
        {
            _transactionEventBusQuery = query;
            _eventType = eventType;
        }

        public void DispatchEvent()
        {
            var eventBuffer = _transactionEventBusQuery.GetSingletonBuffer<TransactionEventBuffer>();
            eventBuffer.Add(new TransactionEventBuffer { EventType = _eventType });
        }
    }
}

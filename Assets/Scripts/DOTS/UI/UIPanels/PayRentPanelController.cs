using Unity.Entities;

public struct PayRentPanelContext
{
    public int Rent;
}

public class PayRentPanelController
{
    public PayRentPanel Panel { get; private set; }
    public EntityQuery TransactionEventBusQuery { get; private set; }
    public PayRentPanelContext Context { get; set; }

    public PayRentPanelController(PayRentPanel panel, PayRentPanelContext context)
    {
        Panel = panel;
        Context = context;
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        Panel.AcceptButton.clickable.clicked += DispatchEvents;
    }

    public void Update()
    {
        Panel.UpdateRentAmountLabel(Context.Rent.ToString());
    }

    public void SetTransactionEventBusQuery(EntityQuery query)
    {
        TransactionEventBusQuery = query;
    }

    private void DispatchEvents()
    {
        var eventQueue = TransactionEventBusQuery.GetSingletonRW<TransactionEventBus>().ValueRW.EventQueue;
        eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventType.PayRent });
        // TODO: Remove this, we don't want to change turns after paying rent.
        eventQueue.Enqueue(new TransactionEvent { EventType = TransactionEventType.ChangeTurn });
        Panel.Hide();
    }

    public void Dispose()
    {
        Panel.AcceptButton.clickable.clicked -= DispatchEvents;
    }
}


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
        Panel.AcceptButton.clickable.clicked += Panel.Hide;
    }

    public void Update()
    {
        Panel.UpdateRentAmountLabel(Context.Rent.ToString());
    }

    public void SetEventBufferQuery(EntityQuery query)
    {
        TransactionEventBusQuery = query;
    }

    private void DispatchEvents()
    {
        var eventBuffer = TransactionEventBusQuery.GetSingletonBuffer<TransactionEventBuffer>();
        eventBuffer.Add(new TransactionEventBuffer { EventType = TransactionEventType.PayRent });
        // TODO: Remove this, we don't want to change turns after paying rent.
        eventBuffer.Add(new TransactionEventBuffer { EventType = TransactionEventType.ChangeTurn });
    }

    public void Dispose()
    {
        Panel.AcceptButton.clickable.clicked -= DispatchEvents;
        Panel.AcceptButton.clickable.clicked -= Panel.Hide;
    }
}


using Unity.Entities;

public class ChangeTurnPanelController
{
    public ChangeTurnPanel ChangeTurnPanel { get; private set; }
    public EntityQuery TransactionEventBufferQuery { get; private set; }
    public ChangeTurnPanelController(ChangeTurnPanel panel)
    {
        ChangeTurnPanel = panel;
        SubscribeEvents();
    }

    public void SubscribeEvents()
    {
        ChangeTurnPanel.ChangeTurnButton.clickable.clicked += DispatchEvents;
    }

    public void SetEventBufferQuery(EntityQuery query) => TransactionEventBufferQuery = query;

    private void DispatchEvents()
    {
        if (TransactionEventBufferQuery != null)
        {
            var eventBuffer = TransactionEventBufferQuery.GetSingletonBuffer<TransactionEventBuffer>();
            eventBuffer.Add(new TransactionEventBuffer{ EventType = TransactionEventType.ChangeTurn });
        }
    }

    public void Dispose()
    {
        ChangeTurnPanel.ChangeTurnButton.clickable.clicked -= DispatchEvents;
    }
}

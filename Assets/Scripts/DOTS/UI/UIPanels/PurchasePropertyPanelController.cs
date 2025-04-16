using Unity.Entities;

public struct PurchasePropertyPanelContext
{
    public Entity spaceEntity;
    public EntityManager entityManager;
    public int playerID;
}

public class PurchasePropertyPanelController
{
    public PurchasePropertyPanel PurchasePropertyPanel { get; private set; }
    public PurchasePropertyPanelContext Context { get; set; } 
    private EntityQuery transactionEventQuery;

    public PurchasePropertyPanelController(PurchasePropertyPanel purchasePropertyPanel, PurchasePropertyPanelContext context)
    {
        PurchasePropertyPanel = purchasePropertyPanel;
        Context = context;
    }

    public void Update()
    {
        var name = Context.entityManager.GetComponentData<NameComponent>(Context.spaceEntity);
        var price = Context.entityManager.GetComponentData<PriceComponent>(Context.spaceEntity);
        PurchasePropertyPanel.UpdateTitleLabelText(name.Value.ToString());
        PurchasePropertyPanel.UpdatePriceLabelText(price.Value.ToString());
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        PurchasePropertyPanel.AcceptButton.clickable.clicked += DispatchEvents;
    }

    private void DispatchEvents()
    {
        var eventQueue = transactionEventQuery.GetSingletonRW<TransactionEventBus>().ValueRW.EventQueue;
        UnityEngine.Debug.Log("Dispatching buy property event");
        eventQueue.Enqueue(new TransactionEvent{ EventType = TransactionEventType.Purchase });
        PurchasePropertyPanel.Hide();
    }

    public void SetTransactionEventQuery(EntityQuery query)
    {
        transactionEventQuery = query;
    }

    public void Dispose()
    {
        PurchasePropertyPanel.AcceptButton.clickable.clicked -= DispatchEvents;
        PurchasePropertyPanel.Dispose();
    }
}

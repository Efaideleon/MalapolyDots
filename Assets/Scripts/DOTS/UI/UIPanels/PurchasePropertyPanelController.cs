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

    public void SetTransactionEventQuery(EntityQuery query)
    {
        transactionEventQuery = query;
        PurchasePropertyPanel.AddAcceptButtonAction(transactionEventQuery);
    }

    public void Dispose()
    {
        PurchasePropertyPanel.Dispose();
    }
}

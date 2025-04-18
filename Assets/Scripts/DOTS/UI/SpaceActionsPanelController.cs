public struct SpaceActionsPanelContext
{
    public bool IsPlayerOwner;
    public bool HasMonopoly;
}

public class SpaceActionsPanelController
{
    public SpaceActionsPanelContext Context { get; set; }
    public SpaceActionsPanel SpaceActionsPanel { get; private set; }
    public NoMonopolyYetPanel NoMonopolyYetPanel { get; private set; }
    public PurchaseHousePanelController PurchaseHousePanelController { get; private set; }
    public PurchasePropertyPanelController PurchasePropertyPanelController { get; private set; }

    public SpaceActionsPanelController(
            SpaceActionsPanelContext context,
            SpaceActionsPanel panel,
            PurchaseHousePanelController purchaseHousePanelController,
            NoMonopolyYetPanel noMonopolyYetPanel,
            PurchasePropertyPanelController purchasePropertyPanelController)
    {
        if (panel == null || 
            purchaseHousePanelController == null || 
            noMonopolyYetPanel == null ||
            purchasePropertyPanelController == null)
        {
            UnityEngine.Debug.LogWarning("Panel or PurchaseHousePanel is null");
        }
        else
        {
            SpaceActionsPanel = panel;
            PurchaseHousePanelController = purchaseHousePanelController; 
            NoMonopolyYetPanel = noMonopolyYetPanel;
            PurchasePropertyPanelController = purchasePropertyPanelController;
            Context = context;
            SubscribeEvents();
        }
    }

    private void SubscribeEvents()
    {
        SpaceActionsPanel.BuyHouseButton.clickable.clicked += HandleBuyHouseButtonClick;
        SpaceActionsPanel.BuyPropertyButton.clickable.clicked += PurchasePropertyPanelController.ShowPanel;
        NoMonopolyYetPanel.GotItButton.clickable.clicked += NoMonopolyYetPanel.Hide;
    }

    public void Update()
    {
        // TODO: Here we'll change how the icon looks like
    }

    private void HandleBuyHouseButtonClick()
    {
        // TODO: Create a context for this controller too, based on the context the buybutton look and behavior will change
        switch (Context.HasMonopoly)
        {
            case true:
                PurchaseHousePanelController.ResetNumberOfHouseToBuy();
                PurchaseHousePanelController.ShowPanel();
                break;
            case false:
                NoMonopolyYetPanel.Show();
                break;
        }
    }

    public void Dispose()
    {
        SpaceActionsPanel.BuyHouseButton.clickable.clicked -= HandleBuyHouseButtonClick;
        NoMonopolyYetPanel.GotItButton.clickable.clicked -= NoMonopolyYetPanel.Hide;
        SpaceActionsPanel.BuyPropertyButton.clickable.clicked -= PurchasePropertyPanelController.ShowPanel;
    }
}

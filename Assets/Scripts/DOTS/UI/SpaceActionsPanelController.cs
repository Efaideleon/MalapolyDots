using Assets.Scripts.DOTS.UI.UIPanels;

public struct SpaceActionsPanelContext
{
    public bool IsPlayerOwner;
    public bool HasMonopoly;
}

public class SpaceActionsPanelController
{
    public SpaceActionsPanel SpaceActionsPanel { get; private set; }
    public PurchaseHousePanel PurchaseHousePanel { get; private set; }
    public NoMonopolyYetPanel NoMonopolyYetPanel { get; private set; }
    public PurchasePropertyPanel PurchasePropertyPanel { get; private set; }
    public SpaceActionsPanelContext Context { get; set; }

    public SpaceActionsPanelController(
            SpaceActionsPanelContext context,
            SpaceActionsPanel panel,
            PurchaseHousePanel purchaseHousePanel,
            NoMonopolyYetPanel noMonopolyYetPanel,
            PurchasePropertyPanel buyPropertyPanel)
    {
        if (panel == null || 
            purchaseHousePanel == null || 
            noMonopolyYetPanel == null ||
            buyPropertyPanel == null)
        {
            UnityEngine.Debug.LogWarning("Panel or PurchaseHousePanel is null");
        }
        else
        {
            SpaceActionsPanel = panel;
            PurchaseHousePanel = purchaseHousePanel; 
            NoMonopolyYetPanel = noMonopolyYetPanel;
            PurchasePropertyPanel = buyPropertyPanel;
            Context = context;
            SubscribeEvents();
        }
    }

    private void SubscribeEvents()
    {
        SpaceActionsPanel.BuyHouseButton.clickable.clicked += HandleBuyButtonClick;
        SpaceActionsPanel.BuyPropertyButton.clickable.clicked += PurchasePropertyPanel.Show;
        NoMonopolyYetPanel.GotItButton.clickable.clicked += NoMonopolyYetPanel.Hide;
    }

    public void Update()
    {
        // TODO: Here we'll change how the icon looks like
    }

    private void HandleBuyButtonClick()
    {
        // TODO: Create a context for this controller too, based on the context the buybutton look and behavior will change
        if (!Context.HasMonopoly)
        {
            NoMonopolyYetPanel.Show();
        }
        else
        {
            PurchaseHousePanel.Show();
        }
    }

    public void Dispose()
    {
        SpaceActionsPanel.BuyHouseButton.clickable.clicked -= HandleBuyButtonClick;
        NoMonopolyYetPanel.GotItButton.clickable.clicked -= NoMonopolyYetPanel.Hide;
        SpaceActionsPanel.BuyPropertyButton.clickable.clicked -= PurchasePropertyPanel.Show;
    }
}

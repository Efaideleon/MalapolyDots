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
    public SpaceActionsPanelContext Context { get; set; }

    public SpaceActionsPanelController(
            SpaceActionsPanelContext context,
            SpaceActionsPanel panel,
            PurchaseHousePanel purchaseHousePanel,
            NoMonopolyYetPanel noMonopolyYetPanel)
    {
        if (panel == null || purchaseHousePanel == null || noMonopolyYetPanel == null)
        {
            UnityEngine.Debug.LogWarning("Panel or PurchaseHousePanel is null");
        }
        else
        {
            SpaceActionsPanel = panel;
            PurchaseHousePanel = purchaseHousePanel; 
            NoMonopolyYetPanel = noMonopolyYetPanel;
            Context = context;
            SubscribeEvents();
        }
    }

    private void SubscribeEvents()
    {
        // TODO: Deterime if there is monopoly on the proporty to know what panel to show
        // PurchaseHousePanel or NoMonopolyYetPanel
        // This controller needs to now the state of the monopoly
        // Create a context for this controller too, based on the context the buybutton look and behavior will change
        SpaceActionsPanel.BuyButton.clickable.clicked += HandleBuyButtonClick;
    }

    public void Update()
    {
        // TODO: Here we'll change how the icon looks like
    }

    private void HandleBuyButtonClick()
    {
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
        SpaceActionsPanel.BuyButton.clickable.clicked -= HandleBuyButtonClick;
    }
}

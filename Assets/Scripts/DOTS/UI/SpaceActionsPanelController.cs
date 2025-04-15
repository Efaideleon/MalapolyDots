using Assets.Scripts.DOTS.UI.UIPanels;

public class SpaceActionsPanelController
{
    public SpaceActionsPanel SpaceActionsPanel { get; private set; }
    public PurchaseHousePanel PurchaseHousePanel { get; private set; }
    public SpaceActionsPanelController(SpaceActionsPanel panel, PurchaseHousePanel purchaseHousePanel)
    {
        if (panel == null || purchaseHousePanel == null)
        {
            UnityEngine.Debug.LogWarning("Panel or PurchaseHousePanel is null");
        }
        else
        {
            SpaceActionsPanel = panel;
            PurchaseHousePanel = purchaseHousePanel; 
            SubscribeEvents();
        }
    }

    private void SubscribeEvents()
    {
        SpaceActionsPanel.BuyButton.clickable.clicked += PurchaseHousePanel.Show;
    }

    public void Dispose()
    {
        SpaceActionsPanel.BuyButton.clickable.clicked -= PurchaseHousePanel.Show;
    }
}

using System.Collections.Generic;
using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;

// Legacy Code
/*
public class BuyHouseUIController
{
    public List<string> AvailableProperties { get; private set; }
    public BuyHouseUI BuyHouseUI { get; private set; }
    private EntityQuery buyHouseEventsQuery;
    private readonly PropertyPurchasePanelFactory purchasePanelFactory;

    public BuyHouseUIController(BuyHouseUI buyHouseUI)
    {
        BuyHouseUI = buyHouseUI;
        AvailableProperties = new();
        purchasePanelFactory = new();
        SubscribeEvents();
    }

    // once all the house have been bought for a property remove that panel or gray it out.
    // so that no more house be bought for it.
    public void RegisterPropertyName(string name)
    {
        UnityEngine.Debug.Log($"Add Property Name {name}");
        AvailableProperties.Add(name);
    }

    public void OpenPanel()
    {
        // Check if the current player has any monopolies over a color
        // if they do then create the panel to buy a house
        foreach (var name in AvailableProperties)
        {
            var propertyContext = new PurchaseHousePanelContext
            {
                Name = name,
                Price = 0,
            };

            // Instantiating the uxml
            var panelVE = purchasePanelFactory.InstantiatePanel(BuyHouseUI.Root);
            var panel = new PurchaseHousePanel(panelVE, propertyContext);
            panel.OnOkClicked += DispatchHousePurchaseEvents;
            // Keeping track of how many buy/sell property elements we instantiated
            // When do I clear this list? when no properties can be bought anymore?
            BuyHouseUI.ListOfPurchasePanels.Add(panel);
        }
        BuyHouseUI.BuyHousePanel.Show();
    }

    public void ClosePanel()
    {
        BuyHouseUI.BuyHousePanel.Hide();
        foreach (var panel in BuyHouseUI.ListOfPurchasePanels)
        {
            panel.Dispose();
        }
        AvailableProperties.Clear();
    }

    private void DispatchHousePurchaseEvents(ToggleState toggleState, PurchaseHousePanelContext context)
    {
        switch(toggleState)
        {
            case ToggleState.Buy:
                var eventBuffer = buyHouseEventsQuery.GetSingletonBuffer<BuyHouseEvent>();
                foreach (var PurchaseEvent in GeneratePurchaseEvents(context))
                {
                    UnityEngine.Debug.Log($"Buy a house for {PurchaseEvent.property}");
                    eventBuffer.Add(PurchaseEvent);
                }
                break;
            case ToggleState.Sell:
                UnityEngine.Debug.Log("Selling abel houses lol");
                break;
        }
    }

    public void SetBuyHouseEventQuery(EntityQuery entityQuery)
    {
        buyHouseEventsQuery = entityQuery;
    }

    private List<BuyHouseEvent> GeneratePurchaseEvents(PurchaseHousePanelContext context)
    {
        List<BuyHouseEvent> listOfBuyHouseEvents = new();
        UnityEngine.Debug.Log($"Buying hosues for {context.Name}");
        foreach (var panel in BuyHouseUI.ListOfPurchasePanels)
        {
            if (context.Name == panel.Context.Name)
            {
                for (int i = 0; i < panel.NumOfHousesToBuy; i++)
                {
                    listOfBuyHouseEvents.Add(new BuyHouseEvent { property = panel.Context.Name });
                }
            }
        }
        return listOfBuyHouseEvents;
    }

    private void SubscribeEvents()
    {
        BuyHouseUI.buyHouseButton.clickable.clicked += OpenPanel;
        BuyHouseUI.closePanel.clickable.clicked += ClosePanel;
    }

    public void Dispose()
    {
        BuyHouseUI.buyHouseButton.clickable.clicked -= OpenPanel;
        BuyHouseUI.closePanel.clickable.clicked -= ClosePanel;
    }
}
*/

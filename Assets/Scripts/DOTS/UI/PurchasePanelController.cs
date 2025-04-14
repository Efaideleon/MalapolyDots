using System.Collections.Generic;
using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;

public class PurchasePanelController
{
    private EntityQuery buyHouseEventsQuery;
    public PropertyPurchasePanel PurchasePanel { get; private set; }

    public PurchasePanelController(PropertyPurchasePanel purchasePanel)
    {
        PurchasePanel = purchasePanel;
        SubscribeEvents();
    }

    public void SetBuyHouseEventQuery(EntityQuery entityQuery)
    {
        buyHouseEventsQuery = entityQuery;
    }

    private void SubscribeEvents()
    {
        PurchasePanel.OnOkClicked += DispatchHousePurchaseEvents;
    }

    public void Dispose()
    {
        PurchasePanel.OnOkClicked -= DispatchHousePurchaseEvents;
    }

    private void DispatchHousePurchaseEvents(ToggleState toggleState, PropertyPurchasePanelContext context)
    {
        switch(toggleState)
        {
            case ToggleState.Buy:
                if (buyHouseEventsQuery != null)
                {
                    var eventBuffer = buyHouseEventsQuery.GetSingletonBuffer<BuyHouseEvent>();
                    foreach (var PurchaseEvent in GeneratePurchaseEvents(context))
                    {
                        UnityEngine.Debug.Log($"Buy a house for {PurchaseEvent.property}");
                        eventBuffer.Add(PurchaseEvent);
                    }
                }
                else 
                {
                    UnityEngine.Debug.Log("buyHouseEventsQuery not set in PurchasePanelController");
                }
                break;
            case ToggleState.Sell:
                UnityEngine.Debug.Log("Selling abel houses lol");
                break;
        }
    }

    private List<BuyHouseEvent> GeneratePurchaseEvents(PropertyPurchasePanelContext context)
    {
        List<BuyHouseEvent> listOfBuyHouseEvents = new();
        UnityEngine.Debug.Log($"Buying hosues for {context.Name}");

        if (context.Name == PurchasePanel.Context.Name)
        {
            for (int i = 0; i < PurchasePanel.NumOfHousesToBuy; i++)
            {
                listOfBuyHouseEvents.Add(new BuyHouseEvent { property = PurchasePanel.Context.Name });
            }
        }
        return listOfBuyHouseEvents;
    }
}

using System.Collections.Generic;
using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;

public class PurchasePanelController
{
    private EntityQuery buyHouseEventBufferQuery;
    public PurchaseHousePanel PurchaseHousePanel { get; private set; }

    public PurchasePanelController(PurchaseHousePanel purchasePanel)
    {
        PurchaseHousePanel = purchasePanel;
        SubscribeEvents();
    }

    public void SetBuyHouseEventQuery(EntityQuery entityQuery)
    {
        buyHouseEventBufferQuery = entityQuery;
    }

    private void SubscribeEvents()
    {
        PurchaseHousePanel.OnOkClicked += DispatchHousePurchaseEvents;
    }

    public void Dispose()
    {
        PurchaseHousePanel.OnOkClicked -= DispatchHousePurchaseEvents;
    }

    private void DispatchHousePurchaseEvents(ToggleState toggleState, PurchaseHousePanelContext context)
    {
        switch(toggleState)
        {
            case ToggleState.Buy:
                if (buyHouseEventBufferQuery != null)
                {
                    var eventBuffer = buyHouseEventBufferQuery.GetSingletonBuffer<BuyHouseEvent>();
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

    private List<BuyHouseEvent> GeneratePurchaseEvents(PurchaseHousePanelContext context)
    {
        List<BuyHouseEvent> listOfBuyHouseEvents = new();
        UnityEngine.Debug.Log($"Buying hosues for {context.Name}");

        if (context.Name == PurchaseHousePanel.Context.Name)
        {
            for (int i = 0; i < PurchaseHousePanel.NumOfHousesToBuy; i++)
            {
                listOfBuyHouseEvents.Add(new BuyHouseEvent { property = PurchaseHousePanel.Context.Name });
            }
        }
        return listOfBuyHouseEvents;
    }
}

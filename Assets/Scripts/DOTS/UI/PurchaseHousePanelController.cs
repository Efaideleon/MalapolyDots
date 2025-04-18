using System.Collections.Generic;
using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;

public class PurchaseHousePanelController
{
    private EntityQuery buyHouseEventBufferQuery;
    public PurchaseHousePanel PurchaseHousePanel { get; private set; }
    public PurchaseHousePanelController(PurchaseHousePanel purchasePanel)
    {
        PurchaseHousePanel = purchasePanel;
        SubscribeEvents();
    }

    public void ShowPanel() => PurchaseHousePanel.Show();

    public void SetBuyHouseEventQuery(EntityQuery entityQuery)
    {
        buyHouseEventBufferQuery = entityQuery;
    }

    public void ResetNumberOfHouseToBuy() => PurchaseHousePanel.ResetNumOfHousesToBuy();

    private void SubscribeEvents()
    {
        PurchaseHousePanel.OnOkClicked += DispatchHousePurchaseEvents;
    }

    public void Dispose()
    {
        PurchaseHousePanel.OnOkClicked -= DispatchHousePurchaseEvents;
    }

    private void DispatchHousePurchaseEvents(ToggleState toggleState, int numOfHousesToBuy)
    {
        switch(toggleState)
        {
            case ToggleState.Buy:
                if (buyHouseEventBufferQuery != null)
                {
                    var eventBuffer = buyHouseEventBufferQuery.GetSingletonBuffer<BuyHouseEvent>();
                    foreach (var PurchaseEvent in GeneratePurchaseEvents(numOfHousesToBuy))
                    {
                        eventBuffer.Add(PurchaseEvent);
                    }
                }
                else 
                {
                    UnityEngine.Debug.LogWarning("buyHouseEventsQuery not set in PurchasePanelController");
                }
                break;
            case ToggleState.Sell:
                UnityEngine.Debug.Log("Selling abel houses lol");
                break;
        }
    }

    private List<BuyHouseEvent> GeneratePurchaseEvents(int numOfHousesToBuy)
    {
        List<BuyHouseEvent> listOfBuyHouseEvents = new();
        for (int i = 0; i < numOfHousesToBuy; i++)
        {
            listOfBuyHouseEvents.Add(new BuyHouseEvent { property = PurchaseHousePanel.Context.Name });
        }
        return listOfBuyHouseEvents;
    }
}

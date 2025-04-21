using System.Collections.Generic;
using DOTS.GamePlay;
using DOTS.UI.CustomVisualElements;
using DOTS.UI.Panels;
using Unity.Entities;

namespace DOTS.UI.Controllers
{
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

        public void SetEventBufferQuery(EntityQuery entityQuery)
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
                        var eventBuffer = buyHouseEventBufferQuery.GetSingletonBuffer<BuyHouseEventBuffer>();
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

        private List<BuyHouseEventBuffer> GeneratePurchaseEvents(int numOfHousesToBuy)
        {
            List<BuyHouseEventBuffer> listOfBuyHouseEvents = new();
            for (int i = 0; i < numOfHousesToBuy; i++)
            {
                listOfBuyHouseEvents.Add(new BuyHouseEventBuffer { property = PurchaseHousePanel.Context.Name });
            }
            return listOfBuyHouseEvents;
        }
    }
}

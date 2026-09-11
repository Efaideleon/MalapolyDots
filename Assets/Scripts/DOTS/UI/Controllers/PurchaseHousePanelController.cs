using System.Collections.Generic;
using DOTS.EventBuses;
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
                        if (numOfHousesToBuy > 0)
                            eventBuffer.Add(new BuyHouseEventBuffer
                            {
                                PropertyId = PurchaseHousePanel.Context.PropertyId,
                                Count = numOfHousesToBuy
                            });
                    }
                    else 
                    {
                        UnityEngine.Debug.LogWarning("[PurchaseHousePanelController] | buyHouseEventsQuery not set in PurchasePanelController");
                    }
                    break;
                case ToggleState.Sell:
                    UnityEngine.Debug.Log("[PurchaseHousePanelController] | Selling a houses ");
                    break;
            }
        }

    }
}

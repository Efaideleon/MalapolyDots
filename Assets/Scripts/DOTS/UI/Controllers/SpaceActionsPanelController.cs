using DOTS.UI.Panels;
using DOTS.UI.Panels.HideAndShowPanelStateMachineComponents;
using DOTS.UI.Panels.Interfaces;
using DOTS.UI.Utilities.UIButtonEvents;
using UnityEngine.UIElements;

namespace DOTS.UI.Controllers
{
    public struct SpaceActionsPanelContext
    {
        public bool IsPlayerOwner;
        public bool HasMonopoly;
    }

    public class SpaceActionsPanelController : IPanelController
    {
        public SpaceActionsPanelContext Context { get; set; }
        public SpaceActionsPanel SpaceActionsPanel { get; private set; }
        public NoMonopolyYetPanel NoMonopolyYetPanel { get; private set; }
        public PurchaseHousePanelController PurchaseHousePanelController { get; private set; }
        public PurchasePropertyPanelController PurchasePropertyPanelController { get; private set; }
        private readonly HideAndShowPanelStateMachine _hideAndShowStateMachine;
        private readonly IButtonEvent _setUIButtonFlag; 

        public SpaceActionsPanelController(
                SpaceActionsPanelContext context,
                SpaceActionsPanel panel,
                PurchaseHousePanelController purchaseHousePanelController,
                NoMonopolyYetPanel noMonopolyYetPanel,
                PurchasePropertyPanelController purchasePropertyPanelController,
                IButtonEvent setUIButtonFlag)
        {
            if (panel == null ||
                    purchaseHousePanelController == null ||
                    noMonopolyYetPanel == null ||
                    purchasePropertyPanelController == null)
            {
                UnityEngine.Debug.LogWarning("[SpaceActionsPanelController] | Panel or PurchaseHousePanel is null");
            }
            else
            {
                _setUIButtonFlag = setUIButtonFlag;
                SpaceActionsPanel = panel;
                PurchaseHousePanelController = purchaseHousePanelController;
                NoMonopolyYetPanel = noMonopolyYetPanel;
                PurchasePropertyPanelController = purchasePropertyPanelController;
                _hideAndShowStateMachine = new(SpaceActionsPanel, SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.PayRent].Button);
                Context = context;
                SubscribeEvents();
            }
        }

        private void SubscribeEvents()
        {
            // Need a better way to add setUIButtonFlag event to all ui buttons
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHouse].Button.RegisterCallback<MouseUpEvent>(HandleBuyHouseButton, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Button.RegisterCallback<MouseUpEvent>(ShowPropertyPanel, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHouse].Button.RegisterCallback<MouseDownEvent>(SetUIButtonFlag, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Button.RegisterCallback<MouseDownEvent>(SetUIButtonFlag, TrickleDown.TrickleDown);
            NoMonopolyYetPanel.GotItButton.clickable.clicked += NoMonopolyYetPanel.Hide;
        }

        public void ShowPanel()
        {
            _hideAndShowStateMachine.Show();
            Update();
        }
        public void HidePanel()
        {
            _hideAndShowStateMachine.Hide();
        }

        private void ShowPropertyPanel(MouseUpEvent e)
        {
            PurchasePropertyPanelController.ShowPanel(); 
            _setUIButtonFlag.DispatchEvent(); 
        }
        private void HandleBuyHouseButton(MouseUpEvent e)
        {
            HandleBuyHouseButtonClick(); 
            _setUIButtonFlag.DispatchEvent(); 
        }


        private void SetUIButtonFlag(MouseDownEvent e) 
        {
            _setUIButtonFlag.DispatchEvent(); 
        }

        public void Update()
        {
            SpaceActionsPanel.SetHousePurchaseAvailability(Context.IsPlayerOwner && Context.HasMonopoly);
        }

        private void HandleBuyHouseButtonClick()
        {
            if (!Context.IsPlayerOwner || !Context.HasMonopoly) return;
            NoMonopolyYetPanel.Hide();
            PurchaseHousePanelController.ResetNumberOfHouseToBuy();
            PurchaseHousePanelController.ShowPanel();
        }

        public void Dispose()
        {
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHouse].Button.UnregisterCallback<MouseUpEvent>(HandleBuyHouseButton, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Button.UnregisterCallback<MouseUpEvent>(ShowPropertyPanel, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHouse].Button.UnregisterCallback<MouseDownEvent>(SetUIButtonFlag, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Button.UnregisterCallback<MouseDownEvent>(SetUIButtonFlag, TrickleDown.TrickleDown);
            NoMonopolyYetPanel.GotItButton.clickable.clicked -= NoMonopolyYetPanel.Hide;
            _hideAndShowStateMachine.Dispose();
        }
    }
}

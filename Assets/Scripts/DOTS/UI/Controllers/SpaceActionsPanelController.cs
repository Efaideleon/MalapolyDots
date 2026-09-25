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
        public bool CanBuyProperty;
        public bool CanBuyHouse;
        public bool CanBuyHotel;
        public bool MustPayRent;
    }

    public class SpaceActionsPanelController : IPanelController
    {
        public bool IsOpen { get; private set; }
        public SpaceActionsPanelContext Context { get; set; }
        public SpaceActionsPanel SpaceActionsPanel { get; private set; }
        public NoMonopolyYetPanel NoMonopolyYetPanel { get; private set; }
        public PurchaseHousePanelController PurchaseHousePanelController { get; private set; }
        public PurchasePropertyPanelController PurchasePropertyPanelController { get; private set; }
        private readonly HideAndShowPanelStateMachine _hideAndShowStateMachine;
        private readonly PayRentPanelController _payRentPanelController;
        private readonly IButtonEvent _setUIButtonFlag; 

        public SpaceActionsPanelController(
                SpaceActionsPanelContext context,
                SpaceActionsPanel panel,
                PurchaseHousePanelController purchaseHousePanelController,
                NoMonopolyYetPanel noMonopolyYetPanel,
                PurchasePropertyPanelController purchasePropertyPanelController,
                IButtonEvent setUIButtonFlag,
                PayRentPanelController payRentPanelController)
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
                _payRentPanelController = payRentPanelController;
                SpaceActionsPanel = panel;
                PurchaseHousePanelController = purchaseHousePanelController;
                NoMonopolyYetPanel = noMonopolyYetPanel;
                PurchasePropertyPanelController = purchasePropertyPanelController;
                _hideAndShowStateMachine = new(SpaceActionsPanel, SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Container);
                Context = context;
                SubscribeEvents();
            }
        }

        private void SubscribeEvents()
        {
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHotel].Button.clicked += HandleBuyHotelButton;
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.PayRent].Button.clicked += HandlePayRentButton;
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHotel].Button.RegisterCallback<MouseDownEvent>(SetUIButtonFlag, TrickleDown.TrickleDown);
            // Need a better way to add setUIButtonFlag event to all ui buttons
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHouse].Button.RegisterCallback<MouseUpEvent>(HandleBuyHouseButton, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Button.RegisterCallback<MouseUpEvent>(ShowPropertyPanel, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHouse].Button.RegisterCallback<MouseDownEvent>(SetUIButtonFlag, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Button.RegisterCallback<MouseDownEvent>(SetUIButtonFlag, TrickleDown.TrickleDown);
            NoMonopolyYetPanel.GotItButton.clickable.clicked += NoMonopolyYetPanel.Hide;
        }

        public void ShowPanel()
        {
            IsOpen = true;
            Update();
            _hideAndShowStateMachine.Show();
        }
        public void HidePanel()
        {
            IsOpen = false;
            _hideAndShowStateMachine.Hide();
        }

        private void ShowPropertyPanel(MouseUpEvent e)
        {
            if (!Context.CanBuyProperty) return;
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
            SpaceActionsPanel.SetPropertyPurchaseAvailability(Context.CanBuyProperty);
            SpaceActionsPanel.SetHousePurchaseAvailability(Context.CanBuyHouse);
            SpaceActionsPanel.SetHotelPurchaseAvailability(Context.CanBuyHotel);
            SpaceActionsPanel.SetRentPaymentAvailability(Context.MustPayRent);
        }

        private void HandleBuyHouseButtonClick()
        {
            if (!Context.CanBuyHouse) return;
            NoMonopolyYetPanel.Hide();
            PurchaseHousePanelController.ResetNumberOfHouseToBuy();
            PurchaseHousePanelController.ShowPanel();
        }

        private void HandleBuyHotelButton()
        {
            if (!Context.CanBuyHotel) return;
            _setUIButtonFlag.DispatchEvent();
            NoMonopolyYetPanel.Hide();
            PurchaseHousePanelController.ResetNumberOfHouseToBuy();
            PurchaseHousePanelController.ShowPanel();
        }

        private void HandlePayRentButton()
        {
            if (!Context.MustPayRent) return;
            _setUIButtonFlag.DispatchEvent();
            _payRentPanelController.Panel.Show();
        }

        public void Dispose()
        {
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHotel].Button.clicked -= HandleBuyHotelButton;
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.PayRent].Button.clicked -= HandlePayRentButton;
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHotel].Button.UnregisterCallback<MouseDownEvent>(SetUIButtonFlag, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHouse].Button.UnregisterCallback<MouseUpEvent>(HandleBuyHouseButton, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Button.UnregisterCallback<MouseUpEvent>(ShowPropertyPanel, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyHouse].Button.UnregisterCallback<MouseDownEvent>(SetUIButtonFlag, TrickleDown.TrickleDown);
            SpaceActionsPanel.ButtonSet[SpaceActionButtonsEnum.BuyProperty].Button.UnregisterCallback<MouseDownEvent>(SetUIButtonFlag, TrickleDown.TrickleDown);
            NoMonopolyYetPanel.GotItButton.clickable.clicked -= NoMonopolyYetPanel.Hide;
            _hideAndShowStateMachine.Dispose();
        }
    }
}

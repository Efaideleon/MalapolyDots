using System;
using DOTS.UI.CustomVisualElements;
using Unity.Collections;
using UnityEngine.UIElements;
using IPanel = DOTS.UI.Panels.Interfaces.IPanel;

namespace DOTS.UI.Panels
{
    public struct PurchaseHousePanelContext
    {
        public FixedString64Bytes Name { get; set; }
        public int HousesOwned { get; set; }
        public int Price { get; set; }
        public int PropertyId { get; set; }
        public int MaxPurchasable { get; set; }
    }

    public class PurchaseHousePanel : IPanel
    {
        public VisualElement Panel { get; private set; }
        public Label PropertyName { get; private set; }
        public Label BuyingHouseCounter { get; private set; }
        public Label HousesOwnedCounter { get; private set; }
        public Button PlusButton { get; private set; }
        public Button MinusButton { get; private set; }
        public Button OkButton { get; private set; }
        public Button CloseButton { get; private set; }
        public ToggleControl BuySellToggle { get; private set; }

        private Label _priceLabel;
        private int _numOfHousesToBuy; 
        public PurchaseHousePanelContext Context
        {
            get => _context;
            set
            {
                if (_context.PropertyId != value.PropertyId) _numOfHousesToBuy = 0;
                _context = value;
            }
        }
        public Action<ToggleState, int> OnOkClicked;
        private PurchaseHousePanelContext _context;

        public PurchaseHousePanel(VisualElement root, PurchaseHousePanelContext context)
        {
            _context = context;
            Panel = root.Query<VisualElement>("PurchaseHousePanel");
            PropertyName = Panel.Q<Label>("property-name");
            BuyingHouseCounter = Panel.Q<Label>("houses-counter");
            HousesOwnedCounter = Panel.Q<Label>("number-houses-owned");
            MinusButton = Panel.Q<Button>("subtract-houses-amount");
            PlusButton = Panel.Q<Button>("add-houses-amount");
            OkButton = Panel.Q<Button>("ok-button");
            CloseButton = Panel.Q<Button>("purchase-houses-panel-close-button");

            PropertyName.text = _context.Name.ToString();
            BuySellToggle = new ToggleControl(Panel.Q<VisualElement>("toggle-container"));
            _priceLabel = new Label();
            HousesOwnedCounter.parent.Add(_priceLabel);
            // Selling houses is not implemented by the purchase handler.
            Panel.Q<VisualElement>("toggle-container").style.display = DisplayStyle.None;
            Hide();
            SubscribeEvents();
        }

        public void Update()
        {
            UpdateNumOfHousesOwnedLabel(_context.HousesOwned == 5 ? "1 hotel" : $"{_context.HousesOwned} houses");
            UpdatePropertyNameLabel(_context.Name.ToString());
            _numOfHousesToBuy = Math.Min(_numOfHousesToBuy, _context.MaxPurchasable);
            UpdateNumOfHouseToBuyLabel();
        }

        private void UpdateNumOfHousesOwnedLabel(string text)
        {
            if (HousesOwnedCounter != null)
            {
                HousesOwnedCounter.text = text;
            }
            else
            {
                UnityEngine.Debug.Log("[PurchaseHousePanel] | HousesOwnedCounter is null");
            }
        }

        private void UpdatePropertyNameLabel(string text)
        {
            if (PropertyName != null)
            {
                PropertyName.text = text;
            }
            else
            {
                UnityEngine.Debug.Log("[PurchaseHousePanel] | PropertyName is null");
            }
        }

        public void SubscribeEvents()
        {
            if (PlusButton != null)
            {
                PlusButton.clickable.clicked += IncreaseNumOfHouseToBuy;
            }
            else
            {
                UnityEngine.Debug.LogWarning("[PurchaseHousePanel] | Plus button is null");
            }
            if (MinusButton != null)
            {
                MinusButton.clickable.clicked += DecreaseNumOfHouseToBuy;
            }
            else
            {
                UnityEngine.Debug.LogWarning("[PurchaseHousePanel] | Minus button is null");
            }
            if (OkButton != null)
            {
                OkButton.clickable.clicked += HandleOkButtonClicked;
            }
            else
            {
                UnityEngine.Debug.LogWarning("[PurchaseHousePanel] | OkButton is null");
            }
            if (CloseButton != null)
            {
                CloseButton.clickable.clicked += Hide;
            }
            else
            {
                UnityEngine.Debug.LogWarning("[PurchaseHousePanel] | CloseButton is null");
            }
        }

        public void Show() => Panel.style.display = DisplayStyle.Flex;

        public void Hide() => Panel.style.display = DisplayStyle.None;

        public void HandleOkButtonClicked()
        {
            if (_numOfHousesToBuy <= 0 || _numOfHousesToBuy > _context.MaxPurchasable) return;
            switch (BuySellToggle.State)
            {
                case ToggleState.Buy:
                    OnOkClicked?.Invoke(ToggleState.Buy, _numOfHousesToBuy);
                    ResetNumOfHousesToBuy();
                    break;
                case ToggleState.Sell:
                    OnOkClicked?.Invoke(ToggleState.Sell, _numOfHousesToBuy);
                    ResetNumOfHousesToBuy();
                    break;
            }
        }

        // TODO: Must call when the element/BuyHousePanel gets destroyed
        public void Dispose()
        {
            PlusButton.clickable.clicked -= IncreaseNumOfHouseToBuy;
            MinusButton.clickable.clicked -= DecreaseNumOfHouseToBuy;
            OkButton.clickable.clicked -= HandleOkButtonClicked;
            CloseButton.clickable.clicked -= Hide;
            BuySellToggle.Dispose();
        }

        // TODO: Later we may want to send the events to a system to check if a house can be bought at all
        // To prevent increase the number of the UI events it no houses can be bought
        private void IncreaseNumOfHouseToBuy()
        {
            if (_numOfHousesToBuy >= _context.MaxPurchasable) return;
            _numOfHousesToBuy++;
            UpdateNumOfHouseToBuyLabel();
        }

        private void DecreaseNumOfHouseToBuy()
        {
            if (_numOfHousesToBuy <= 0) return;
            _numOfHousesToBuy--;
            UpdateNumOfHouseToBuyLabel();
        }

        public void ResetNumOfHousesToBuy()
        {
            _numOfHousesToBuy = 0;
            UpdateNumOfHouseToBuyLabel();
        }

        private void UpdateNumOfHouseToBuyLabel()
        {
            PlusButton?.SetEnabled(_numOfHousesToBuy < _context.MaxPurchasable);
            MinusButton?.SetEnabled(_numOfHousesToBuy > 0);
            OkButton?.SetEnabled(_numOfHousesToBuy > 0);
            if (_priceLabel != null)
                _priceLabel.text = $"{_context.Price:N0} per {(_context.HousesOwned == 4 ? "hotel" : "house")} · Total: {(long)_context.Price * _numOfHousesToBuy:N0}";
            if (BuyingHouseCounter != null)
            {
                BuyingHouseCounter.text = _numOfHousesToBuy.ToString();
            }
            else
            {
                UnityEngine.Debug.LogWarning("[PurchaseHousePanel] | BuyingHouseCounter Label is null");
            }
        }
    }
}

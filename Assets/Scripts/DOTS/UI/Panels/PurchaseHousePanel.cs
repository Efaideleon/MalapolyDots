using System;
using DOTS.UI.CustomVisualElements;
using Unity.Collections;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public struct PurchaseHousePanelContext
    {
        public FixedString64Bytes Name { get; set; }
        public int HousesOwned { get; set; }
        public int Price { get; set; }
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

        private int _numOfHousesToBuy; 
        public PurchaseHousePanelContext Context { get { return _context; } set { _context = value;} }
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
            Hide();
            SubscribeEvents();
        }

        public void Update()
        {
            UpdateNumOfHousesOwnedLabel(_context.HousesOwned.ToString());
            UpdatePropertyNameLabel(_context.Name.ToString());
        }

        private void UpdateNumOfHousesOwnedLabel(string text)
        {
            if (HousesOwnedCounter != null)
            {
                HousesOwnedCounter.text = text;
            }
            else
            {
                UnityEngine.Debug.Log("HousesOwnedCounter is null");
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
                UnityEngine.Debug.Log("PropertyName is null");
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
                UnityEngine.Debug.LogWarning("Plus button is null");
            }
            if (MinusButton != null)
            {
                MinusButton.clickable.clicked += DecreaseNumOfHouseToBuy;
            }
            else
            {
                UnityEngine.Debug.LogWarning("Minus button is null");
            }
            if (OkButton != null)
            {
                OkButton.clickable.clicked += HandleOkButtonClicked;
            }
            else
            {
                UnityEngine.Debug.LogWarning("OkButton is null");
            }
            if (CloseButton != null)
            {
                CloseButton.clickable.clicked += Hide;
            }
            else
            {
                UnityEngine.Debug.LogWarning("CloseButton is null");
            }
        }

        public void Show() => Panel.style.display = DisplayStyle.Flex;

        public void Hide() => Panel.style.display = DisplayStyle.None;

        public void HandleOkButtonClicked()
        {
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
            _numOfHousesToBuy++;
            UpdateNumOfHouseToBuyLabel();
        }

        private void DecreaseNumOfHouseToBuy()
        {
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
            if (BuyingHouseCounter != null)
            {
                BuyingHouseCounter.text = _numOfHousesToBuy.ToString();
            }
            else
            {
                UnityEngine.Debug.LogWarning("BuyingHouseCounter Label is null");
            }
        }
    }
}

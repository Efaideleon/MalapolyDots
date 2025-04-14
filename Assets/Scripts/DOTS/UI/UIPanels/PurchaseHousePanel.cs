using System;
using Unity.Collections;
using UnityEngine.UIElements;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public struct PurchaseHousePanelContext
    {
        public FixedString64Bytes Name { get; set; }
        public int HousesOwned { get; set; }
        public int Price { get; set; }
    }

    public class PurchaseHousePanel
    {
        public VisualElement Root { get; private set; }
        public Label PropertyName { get; private set; }
        public Label BuyingHouseCounter { get; private set; }
        public Label HousesOwnedCounter { get; private set; }
        public Button PlusButton { get; private set; }
        public Button MinusButton { get; private set; }
        public Button OkButton { get; private set; }
        public ToggleControl BuySellToggle { get; private set; }

        public int NumOfHousesToBuy { get; private set; }
        public PurchaseHousePanelContext Context { get { return _context; } set { _context = value;} }
        public Action<ToggleState, PurchaseHousePanelContext> OnOkClicked;
        private PurchaseHousePanelContext _context;

        public PurchaseHousePanel(VisualElement root, PurchaseHousePanelContext context)
        {
            _context = context;
            Root = root.Query<VisualElement>("PurchaseHousePanel");
            PropertyName = Root.Q<Label>("property-name");
            BuyingHouseCounter = Root.Q<Label>("houses-counter");
            HousesOwnedCounter = Root.Q<Label>("number-houses-owned");
            MinusButton = Root.Q<Button>("subtract-houses-amount");
            PlusButton = Root.Q<Button>("add-houses-amount");
            OkButton = Root.Q<Button>("ok-button");

            PropertyName.text = _context.Name.ToString();
            BuySellToggle = new ToggleControl(Root.Q<VisualElement>("toggle-container"));
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
        }

        public void Show() => Root.style.display = DisplayStyle.Flex;
        public void Hide() => Root.style.display = DisplayStyle.None;

        public void HandleOkButtonClicked()
        {
            switch (BuySellToggle.State)
            {
                case ToggleState.Buy:
                    OnOkClicked?.Invoke(ToggleState.Buy, _context);
                    break;
                case ToggleState.Sell:
                    OnOkClicked?.Invoke(ToggleState.Sell, _context);
                    break;
            }
        }

        // TODO: Must call when the element/BuyHousePanel gets destroyed
        public void Dispose()
        {
            PlusButton.clickable.clicked -= IncreaseNumOfHouseToBuy;
            MinusButton.clickable.clicked -= DecreaseNumOfHouseToBuy;
            OkButton.clickable.clicked -= HandleOkButtonClicked;
            BuySellToggle.Dispose();
        }

        // TODO: Later we may want to send the events to a system to check if a house can be bought at all
        // To prevent increase the number of the UI events it no houses can be bought
        private void IncreaseNumOfHouseToBuy()
        {
            NumOfHousesToBuy++;
            UpdateNumOfHouseToBuyLabel();
        }

        private void DecreaseNumOfHouseToBuy()
        {
            NumOfHousesToBuy--;
            UpdateNumOfHouseToBuyLabel();
        }

        private void UpdateNumOfHouseToBuyLabel()
        {
            if (BuyingHouseCounter != null)
            {
                BuyingHouseCounter.text = NumOfHousesToBuy.ToString();
            }
            else
            {
                UnityEngine.Debug.LogWarning("BuyingHouseCounter Label is null");
            }
        }
    }
}

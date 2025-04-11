using System;
using Unity.Collections;
using UnityEngine.UIElements;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public struct PropertyNameCounterElementContext
    {
        public FixedString64Bytes Name { get; set; }
        public int HousesOwned {get; set; }
        public int Price { get; set; }
    }

    public class PropertyNameCounterElement
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
        public PropertyNameCounterElementContext Context {get; set; }
        public Action<ToggleState, PropertyNameCounterElementContext> OnOkClicked;

        public PropertyNameCounterElement(VisualElement root, PropertyNameCounterElementContext context)
        {
            Context = context;
            Root = root;
            PropertyName = Root.Q<Label>("property-name"); 
            BuyingHouseCounter = Root.Q<Label>("houses-counter"); 
            HousesOwnedCounter = Root.Q<Label>("number-houses-owned"); 
            MinusButton = Root.Q<Button>("subtract-houses-amount");
            PlusButton = Root.Q<Button>("add-houses-amount");
            OkButton = Root.Q<Button>("ok-button");

            PropertyName.text = Context.Name.ToString();
            BuySellToggle = new ToggleControl(Root.Q<VisualElement>("toggle-container"));
            SubscribeEvents();
        }

        public void Update()
        {
            UpdateNumOfHousesOwnedLabel(Context.HousesOwned.ToString());
        }

        private void UpdateNumOfHousesOwnedLabel(string text)
        {
            HousesOwnedCounter.text = text;
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

        public void HandleOkButtonClicked()
        {
            switch(BuySellToggle.State)
            {
                case ToggleState.Buy:
                    OnOkClicked?.Invoke(ToggleState.Buy, Context);
                    break;
                case ToggleState.Sell:
                    OnOkClicked?.Invoke(ToggleState.Sell, Context);
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

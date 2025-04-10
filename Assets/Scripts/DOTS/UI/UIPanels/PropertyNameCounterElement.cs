using Unity.Collections;
using UnityEngine.UIElements;

namespace Assets.Scripts.DOTS.UI.UIPanels
{
    public struct PropertyNameCounterElementContext
    {
        public FixedString64Bytes Name { get; set; }
        public int Price { get; set; }
    }

    public class PropertyNameCounterElement
    {
        public VisualElement Root { get; private set; }
        public Label PropertyName { get; private set; }
        public Label HouseCounter { get; private set; }
        public Button PlusButton { get; private set; }
        public Button MinusButton { get; private set; }
        public int NumOfHousesToBuy { get; private set; }
        public PropertyNameCounterElementContext Context {get; private set; }

        public PropertyNameCounterElement(VisualElement root, PropertyNameCounterElementContext context)
        {
            Context = context;
            Root = root;
            PropertyName = Root.Q<Label>("property-name"); 
            HouseCounter = Root.Q<Label>("houses-counter"); 
            MinusButton = Root.Q<Button>("subtract-houses-amount");
            PlusButton = Root.Q<Button>("add-houses-amount");

            PropertyName.text = Context.Name.ToString();
            SubscribeEvents();
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
        }

        // TODO: Must call when the element/BuyHousePanel gets destroyed
        public void Dispose()
        {
            PlusButton.clickable.clicked -= IncreaseNumOfHouseToBuy;
            MinusButton.clickable.clicked -= DecreaseNumOfHouseToBuy;
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
            if (HouseCounter != null)
            {
                HouseCounter.text = NumOfHousesToBuy.ToString();
            }
            else
            {
                UnityEngine.Debug.LogWarning("HouseCounter Label is null");
            }
        }
    }
}

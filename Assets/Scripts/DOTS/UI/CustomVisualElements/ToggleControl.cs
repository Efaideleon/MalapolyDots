using UnityEngine.UIElements;
using UnityEngine;

namespace DOTS.UI.CustomVisualElements
{
    public enum ToggleState
    {
        Buy,
        Sell
    }

    public class ToggleControl
    {
        public VisualElement Root { get; private set; }
        public VisualElement ToggleHighlight { get; private set; }
        public Button BuyButton { get; private set; }
        public Button SellButton { get; private set; }
        public ToggleState State { get; private set; }

        public ToggleControl(VisualElement root)
        {
            if (root != null)
            {
                Root = root;
                ToggleHighlight = Root.Q<VisualElement>("toggle-element");
                BuyButton = Root.Q<Button>("buy-toggle");
                SellButton = Root.Q<Button>("sell-toggle");
                SubscribeEvents();
                InitializeToggle();
            }
            else
            {
                Debug.LogWarning("root is null in ToggleControl");
            }
        }

        public void SubscribeEvents()
        {
            if (BuyButton != null && SellButton != null)
            {
                BuyButton.clickable.clicked += SetToBuy;
                SellButton.clickable.clicked += SetToSell;
            }
            else
            {
                Debug.LogWarning("BuyButton or SellButton is null in ToggleControl in SubscribeEvents");
            }
        }

        public void SetToBuy()
        {
            if (ToggleHighlight == null && BuyButton == null)
            {
                Debug.LogWarning("ToggleHighlight or BuyButton is null");
                return;
            }
            float target = BuyButton.layout.x;
            ToggleHighlight.style.left = new StyleLength(target);
            State = ToggleState.Buy;
            Debug.Log("changing toggle state  to buy");
        }

        public void SetToSell()
        {
            if (ToggleHighlight == null && SellButton == null)
            {
                Debug.LogWarning("ToggleHighlight or SellButton is null");
                return;
            }
            float target = SellButton.layout.x;
            ToggleHighlight.style.left = new StyleLength(target);
            State = ToggleState.Sell;
            Debug.Log("changing state toggle to sell");
        }

        public void InitializeToggle() => SetToBuy();

        public void Dispose()
        {
            if (BuyButton != null && SellButton != null)
            {
                BuyButton.clickable.clicked -= SetToBuy;
                SellButton.clickable.clicked -= SetToSell;
            }
            else
            {
                Debug.LogWarning("BuyButton or SellButton is null in ToggleControl when diposing");
            }
        }
    }
}

using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class SpaceActionsPanel : IPanel
    {
        public VisualElement Panel { get; private set; }
        public VisualElement BuyHouseButtonContainer { get; private set; }
        public VisualElement BuyHotelButtonContainer { get; private set; }
        public VisualElement BuyPropertyButtonContainer { get; private set; }
        public Button BuyHouseButton { get; private set; }
        public Button BuyPropertyButton { get; private set; }
        public Button PayRentButton { get; private set; }

        public SpaceActionsPanel(VisualElement root)
        {
            Panel = root.Q<VisualElement>("SpaceActions");

            BuyHouseButtonContainer = Panel.Q<VisualElement>("buy-houses-btn-container");
            BuyHotelButtonContainer = Panel.Q<VisualElement>("buy-hotel-btn-container");
            BuyPropertyButtonContainer = Panel.Q<VisualElement>("buy-property-btn-container");
            PayRentButton = Panel.Q<Button>("pay-rent");
            BuyHouseButton = Panel.Q<Button>("buy-house-button");
            if (BuyHouseButton == null)
            {
                UnityEngine.Debug.LogWarning("BuyButton is null");
            }
            BuyPropertyButton = Panel.Q<Button>("buy-property-button");
            if (BuyPropertyButton == null)
            {
                UnityEngine.Debug.LogWarning("BuyPropertyButton is null");
            }
            Hide();
        }

        public void Show()
        {
            Panel.style.display = DisplayStyle.Flex;
            // BuyHouseButtonContainer.AddToClassList("animation-translation-from-bottom");
            // BuyHotelButtonContainer.AddToClassList("animation-translation-from-bottom");
            // BuyPropertyButtonContainer.AddToClassList("animation-translation-from-bottom");
            // PayRentButton.AddToClassList("animation-translation-from-bottom");
            UnityEngine.Debug.Log("Showing the SpaceActions panel");
        }

        public void Hide()
        {
            BuyHouseButtonContainer.RemoveFromClassList("animation-translation-from-bottom");
            BuyHotelButtonContainer.RemoveFromClassList("animation-translation-from-bottom");
            BuyPropertyButtonContainer.RemoveFromClassList("animation-translation-from-bottom");
            PayRentButton.RemoveFromClassList("animation-translation-from-bottom");
            Panel.style.display = DisplayStyle.None;
            UnityEngine.Debug.Log("Hiding the SpaceActions panel");
        }
    }
}

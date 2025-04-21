using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class PurchasePropertyPanel
    {
        public VisualElement Panel { get; private set; }
        public Label PriceLabel {get; private set; }
        public Label NameLabel {get; private set; }
        public Button OkButton {get; private set; }

        public PurchasePropertyPanel(VisualElement parent)
        {
            Panel = parent.Q<VisualElement>("PurchasePropertyPanel");
            PriceLabel = Panel.Q<Label>("price-popup-menu-label");
            NameLabel = Panel.Q<Label>("buy-popup-menu-label");
            OkButton = Panel.Q<Button>("popup-menu-accept-button");
            Hide();
        }

        public void Hide() => Panel.style.display = DisplayStyle.None;
        public void Show() => Panel.style.display = DisplayStyle.Flex;
    }
}

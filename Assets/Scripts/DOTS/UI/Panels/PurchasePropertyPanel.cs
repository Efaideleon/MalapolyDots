using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class PurchasePropertyPanel : IPanel
    {
        public VisualElement Panel { get; private set; }
        public Label PriceLabel {get; private set; }
        public Label NameLabel {get; private set; }
        public Button OkButton {get; private set; }
        public Button DeclineButton {get; private set; }
        public VisualElement Image {get; private set; }

        public PurchasePropertyPanel(VisualElement parent)
        {
            Panel = parent.Q<VisualElement>("PurchasePropertyPanel");
            Image = Panel.Q<VisualElement>("property-image");
            NameLabel = Panel.Q<Label>("name-label");
            PriceLabel = Panel.Q<Label>("price-label");
            OkButton = Panel.Q<Button>("purchase-button");
            DeclineButton = Panel.Q<Button>("decline-button");
            Hide();
        }

        public void Hide() => Panel.style.display = DisplayStyle.None;
        public void Show() => Panel.style.display = DisplayStyle.Flex;
    }
}

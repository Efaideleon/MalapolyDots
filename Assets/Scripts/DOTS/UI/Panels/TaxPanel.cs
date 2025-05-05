using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class TaxPanel
    {
        public Button OkButton { get; private set; }
        public Label AmountLabel { get; private set; }
        public VisualElement Root { get; private set; }
        public TaxPanel(VisualElement parent) 
        {
            Root = parent.Q<VisualElement>("TaxPanel");
            OkButton = Root.Q<Button>("tax-panel-button");
            AmountLabel = Root.Q<Label>("tax-panel-label");
            Hide();
        }

        public void Show() => Root.style.display = DisplayStyle.Flex;
        public void Hide() => Root.style.display = DisplayStyle.None;
    }
}

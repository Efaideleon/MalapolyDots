using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class TreasurePanel
    {
        public VisualElement Root { get; private set; }
        public Button OkButton { get; private set; }
        public Label TitleLabel { get; private set; }
        public TreasurePanel(VisualElement parent) 
        {
            Root = parent.Q<VisualElement>("TreasurePanel");
            OkButton = Root.Q<Button>("treasure-panel-button");
            TitleLabel = Root.Q<Label>("treasure-panel-button");
            Hide();
        }

        public void Show() => Root.style.display = DisplayStyle.Flex;
        public void Hide() => Root.style.display = DisplayStyle.None;
    }
}

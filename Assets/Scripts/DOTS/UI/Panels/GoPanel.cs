using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class GoPanel
    {
        public VisualElement Root { get; private set; }
        public Button OkButton { get; private set; }
        public Label TitleLabel { get; private set; }

        public GoPanel(VisualElement parent)
        {
            Root = parent.Q<VisualElement>("GoPanel");
            OkButton = Root.Q<Button>("go-panel-button");
            TitleLabel = Root.Q<Label>("go-panel-label");
            Hide();
        }

        public void Show() => Root.style.display = DisplayStyle.Flex;
        public void Hide() => Root.style.display = DisplayStyle.None;
    }
}

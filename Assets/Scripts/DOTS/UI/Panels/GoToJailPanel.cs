using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class GoToJailPanel
    {
        public VisualElement Root { get; private set; }
        public Button OkButton { get; private set; }
        public Label TitleLabel { get; private set; }

        public GoToJailPanel(VisualElement parent)
        {
            Root = parent.Q<VisualElement>("GoToJailPanel");
            OkButton = Root.Q<Button>("go-to-jail-panel-button");
            TitleLabel = Root.Q<Label>("go-to-jail-panel-label");
            Hide();
        }

        public void Show() => Root.style.display = DisplayStyle.Flex;
        public void Hide() => Root.style.display = DisplayStyle.None;
    }
}

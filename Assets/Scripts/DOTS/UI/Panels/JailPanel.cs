using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class JailPanel
    {
        public VisualElement Root { get; private set; }
        public Button OkButton { get; private set; }
        public Label TitleLabel { get; private set; }
        public JailPanel(VisualElement parent)
        {
            Root = parent.Q<VisualElement>("JailPanel");
            OkButton = parent.Q<Button>("jail-panel-button");
            TitleLabel = parent.Q<Label>("jail-panel-label");
            Hide();
        }

        public void Show() => Root.style.display = DisplayStyle.Flex;
        public void Hide() => Root.style.display = DisplayStyle.None;
    }
}

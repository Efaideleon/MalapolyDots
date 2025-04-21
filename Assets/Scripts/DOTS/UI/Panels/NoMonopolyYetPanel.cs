using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class NoMonopolyYetPanel
    {
        public VisualElement Panel { get; private set; }
        public Button GotItButton { get; private set; }

        public NoMonopolyYetPanel(VisualElement root)
        {
            Panel = root.Q<VisualElement>("NoMonopolyYetPanel");
            GotItButton = Panel.Q<Button>("got-it-button");
            Hide();
        }

        public void Show() => Panel.style.display = DisplayStyle.Flex;
        public void Hide() => Panel.style.display = DisplayStyle.None;
    }
}

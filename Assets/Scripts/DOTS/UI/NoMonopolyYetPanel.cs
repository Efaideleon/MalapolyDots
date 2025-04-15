using UnityEngine.UIElements;

public class NoMonopolyYetPanel
{
    public VisualElement Panel { get; private set; }

    public NoMonopolyYetPanel(VisualElement root)
    {
        Panel = root.Q<VisualElement>("NoMonopolyYetPanel");
        Hide();
    }

    public void Show() => Panel.style.display = DisplayStyle.Flex;
    public void Hide() => Panel.style.display = DisplayStyle.None;
}

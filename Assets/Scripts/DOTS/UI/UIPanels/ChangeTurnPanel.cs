using UnityEngine.UIElements;

public class ChangeTurnPanel
{
    public VisualElement Panel { get; private set; }
    public Button ChangeTurnButton { get; private set; }

    public ChangeTurnPanel(VisualElement parent)
    {
        Panel = parent.Q<VisualElement>("ChangeTurnButton");
        ChangeTurnButton = Panel.Q<Button>("change-turn-button");
    }

    public void Show() => Panel.style.display = DisplayStyle.Flex;
    public void Hide() => Panel.style.display = DisplayStyle.None;
}

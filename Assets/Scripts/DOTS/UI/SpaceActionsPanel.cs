using UnityEngine.UIElements;

public class SpaceActionsPanel
{
    public VisualElement Root { get; private set; }
    public SpaceActionsPanel(VisualElement root)
    {
        Root = root.Q<VisualElement>("SpaceActions");
    }

    public void Show() => Root.style.display = DisplayStyle.Flex;
    public void Hide() => Root.style.display = DisplayStyle.None;
}

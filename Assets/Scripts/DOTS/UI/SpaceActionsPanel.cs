using UnityEngine.UIElements;

public class SpaceActionsPanel
{
    public VisualElement Panel { get; private set; }
    public VisualElement Backdrop { get; private set; }

    public SpaceActionsPanel(VisualElement root)
    {
        Panel = root.Q<VisualElement>("SpaceActions");
        Backdrop = Panel.Q<VisualElement>("Backdrop");
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        Backdrop.RegisterCallback((PointerDownEvent evt) =>
        {
            Hide();
            evt.StopPropagation();
        });
        Panel.RegisterCallback((PointerDownEvent evt) => 
        {
            evt.StopPropagation();
        });
    }

    public void Show()
    {
        Panel.style.display = DisplayStyle.Flex;
        Backdrop.style.display = DisplayStyle.Flex;
    }

    public void Hide() 
    {
        Panel.style.display = DisplayStyle.None;
        Backdrop.style.display = DisplayStyle.None;
    }
}

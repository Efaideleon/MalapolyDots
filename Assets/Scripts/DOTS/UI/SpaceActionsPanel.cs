using UnityEngine.UIElements;

public class SpaceActionsPanel
{
    public VisualElement Panel { get; private set; }

    public SpaceActionsPanel(VisualElement root)
    {
        Panel = root.Q<VisualElement>("SpaceActions");
    //     Panel.RegisterCallback((PointerDownEvent evt) => 
    //     {
    //         evt.StopPropagation();
    //     });
    }

    public void Show() 
    {
        UnityEngine.Debug.Log("Showing the SpaceActions panel");
        Panel.style.display = DisplayStyle.Flex;
    }

    public void Hide() 
    {
        UnityEngine.Debug.Log("Hiding the SpaceActions panel");
        Panel.style.display = DisplayStyle.None; 
    }
}

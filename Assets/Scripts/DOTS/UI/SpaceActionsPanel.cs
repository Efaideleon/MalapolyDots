using UnityEngine.UIElements;

public class SpaceActionsPanel
{
    public VisualElement Panel { get; private set; }
    public Button BuyButton { get; private set; }

    public SpaceActionsPanel(VisualElement root)
    {
        Panel = root.Q<VisualElement>("SpaceActions");
        BuyButton = Panel.Q<Button>("buy-house-button");
        if (BuyButton == null)
        {
            UnityEngine.Debug.LogWarning("BuyButton is null");
        }
        Hide();
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

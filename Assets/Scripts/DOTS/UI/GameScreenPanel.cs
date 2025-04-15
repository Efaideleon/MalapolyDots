using UnityEngine.UIElements;

public class GameScreenPanel
{
    public VisualElement Panel { get; private set; } 
    // TODO: Add the Bot and Top container and Backdrop?

    public GameScreenPanel(VisualElement root)
    {
        Panel = root.Q<VisualElement>("GameScreen");
    }
}

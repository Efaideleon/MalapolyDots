using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class GameScreenPanel
    {
        public VisualElement Panel { get; private set; } 
        // TODO: Add the Bot and Top container and Backdrop?

        public GameScreenPanel(VisualElement root)
        {
            Panel = root.Q<VisualElement>("GameScreen");
        }
    }
}

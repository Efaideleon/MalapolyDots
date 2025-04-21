using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class SpaceActionsPanel
    {
        public VisualElement Panel { get; private set; }
        public Button BuyHouseButton { get; private set; }
        public Button BuyPropertyButton { get; private set; }

        public SpaceActionsPanel(VisualElement root)
        {
            Panel = root.Q<VisualElement>("SpaceActions");
            BuyHouseButton = Panel.Q<Button>("buy-house-button");
            if (BuyHouseButton == null)
            {
                UnityEngine.Debug.LogWarning("BuyButton is null");
            }
            BuyPropertyButton = Panel.Q<Button>("buy-property-button");
            if (BuyPropertyButton == null)
            {
                UnityEngine.Debug.LogWarning("BuyPropertyButton is null");
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
}

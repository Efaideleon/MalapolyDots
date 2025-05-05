using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class ParkingPanel 
    {
        public VisualElement Root { get; private set; }
        public Button OkButton { get; private set; }
        public Label TitleLabel { get; private set; }

        public ParkingPanel(VisualElement parent) 
        {
            Root = parent.Q<VisualElement>("ParkingPanel");
            OkButton = Root.Q<Button>("parking-panel-button");
            TitleLabel = Root.Q<Label>("parking-panel-label");
            Hide();
        }

        public void Show() => Root.style.display = DisplayStyle.Flex;
        public void Hide() => Root.style.display = DisplayStyle.None;
    }
}

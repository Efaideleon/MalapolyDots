using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class ChancePanel
    {
        public VisualElement Root { get; private set; }
        public Button OkButton { get; private set; } 
        public Label TitleLabel { get; private set; } 

        public ChancePanel(VisualElement parent) 
        {
            Root = parent.Q<VisualElement>("ChancePanel");
            OkButton = Root.Q<Button>("chance-panel-button");
            TitleLabel = Root.Q<Label>("chance-panel-label");
            Hide();
        }
        
        public void Show() => Root.style.display = DisplayStyle.Flex;
        public void Hide() => Root.style.display = DisplayStyle.None;
    }
}

using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class RollPanel 
    {
        public Button RollButton { get; private set; }
        public Label RollLabel { get; private set; }
        public VisualElement Panel { get; private set; }

        public RollPanel(VisualElement parent)
        {
            Panel = parent.Q<VisualElement>("RollPanel");
            RollButton = Panel.Q<Button>("roll-button");
            RollLabel = Panel.Q<Label>("roll-amount-label");
            // Hide();
        }

        public void UpdateRollLabel(string text)
        {
            RollLabel.text = text;
        }

        public void Show() 
        {
            UpdateRollLabel("0");
            Panel.style.display = DisplayStyle.Flex;
            ShowRollButton();
        }
        public void Hide() => Panel.style.display = DisplayStyle.None;

        public void HideRollButton() => RollButton.style.display = DisplayStyle.None;
        public void ShowRollButton() => RollButton.style.display = DisplayStyle.Flex;
    }
}

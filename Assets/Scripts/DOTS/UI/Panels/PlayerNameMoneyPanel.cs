using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class PlayerNameMoneyPanel
    {
        public VisualElement Root { get; private set; }
        public Label PlayerNameLabel { get; private set; }
        public Label PlayerMoneyLabel { get; private set; }
        private readonly VisualElement _container;

        public PlayerNameMoneyPanel(VisualElement root)
        {
            Root = root;
            Root.name = "PlayerNameMoneyPanel";
            if (Root == null)
            {
                UnityEngine.Debug.Log("Stats panels parent is null");
            }

            Root.style.width = StyleKeyword.Auto;
            Root.style.height = StyleKeyword.Auto; 
            Root.pickingMode = PickingMode.Ignore;
            PlayerNameLabel = Root.Q<Label>("player-name");
            PlayerMoneyLabel = Root.Q<Label>("player-money");
            _container = Root.Q<VisualElement>("player-panel");

            DisableHighlightActivePlayerPanel();
        }

        public void UpdatePlayerNameLabelText(string text)
        {
            PlayerNameLabel.text = text;
        }

        public void UpdatePlayerMoneyLabelText(string text)
        {
            PlayerMoneyLabel.text = text;
        }

        public void HighlightActivePlayerPanel()
        {
            Root.style.position = Position.Absolute;
            Root.style.top = 130;
            Root.style.left = new Length(2, LengthUnit.Percent);
            _container.AddToClassList("current-player-panel");
        }

        public void DisableHighlightActivePlayerPanel()
        {
            Root.style.position = Position.Relative;
            Root.style.top = StyleKeyword.Auto;
            Root.style.left = StyleKeyword.Auto;
            _container.RemoveFromClassList("current-player-panel");
        }
    }
}

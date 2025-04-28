using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class PlayerNameMoneyPanel
    {
        public VisualElement Parent { get; private set; }
        public Label PlayerNameLabel { get; private set; }
        public Label PlayerMoneyLabel { get; private set; }

        public PlayerNameMoneyPanel(VisualElement parent)
        {
            Parent = parent;
            if (Parent == null)
            {
                UnityEngine.Debug.Log("Stats panels parent is null");
            }

            PlayerNameLabel = Parent.Q<Label>("player-name");
            PlayerMoneyLabel = Parent.Q<Label>("player-money");
        }

        public void UpdatePlayerNameLabelText(string text)
        {
            PlayerNameLabel.text = text;
        }

        public void UpdatePlayerMoneyLabelText(string text)
        {
            PlayerMoneyLabel.text = text;
        }
    }
}

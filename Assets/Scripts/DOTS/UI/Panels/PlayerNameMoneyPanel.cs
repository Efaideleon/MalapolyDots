using UnityEngine;
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

            UnityEngine.Debug.Log($"player name pane parent: {Parent.name}");
            Parent.style.width = new StyleLength(300);
            Parent.style.height = new StyleLength(300);
            Parent.style.backgroundColor = new StyleColor(Color.red);
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

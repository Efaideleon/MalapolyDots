using UnityEngine;
using UnityEngine.UIElements;

namespace DOTS.UI.Panels
{
    public class PlayerNameMoneyPanel
    {
        public VisualElement Root { get; private set; }
        public Label PlayerNameLabel { get; private set; }
        public Label PlayerMoneyLabel { get; private set; }

        public PlayerNameMoneyPanel(VisualElement root)
        {
            Root = root;
            if (Root == null)
            {
                UnityEngine.Debug.Log("Stats panels parent is null");
            }

            Root.style.width = StyleKeyword.Auto;
            Root.style.height = StyleKeyword.Auto; 
            Root.pickingMode = PickingMode.Ignore;
            PlayerNameLabel = Root.Q<Label>("player-name");
            PlayerMoneyLabel = Root.Q<Label>("player-money");
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

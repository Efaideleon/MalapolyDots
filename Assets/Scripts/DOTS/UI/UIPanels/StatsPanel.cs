using UnityEngine.UIElements;

public class StatsPanel
{
    public VisualElement Parent { get; private set; }
    public Label PlayerNameLabel { get; private set; }
    public Label PlayerMoneyLabel { get; private set; }

    public StatsPanel(VisualElement parent)
    {
        Parent = parent;
        PlayerNameLabel = Parent.Q<Label>("player-name-label");
        PlayerMoneyLabel = Parent.Q<Label>("player-money-label");
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

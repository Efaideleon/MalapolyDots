using UnityEngine.UIElements;

public class NumOfRoundsScreen
{
    private readonly VisualElement _root;
    public readonly Button Button8Rounds;
    public readonly Button Button12Rounds;
    public readonly Button Button16Rounds;
    public Button ConfirmButton { get; private set; }

    public NumOfRoundsScreen(VisualElement root)
    {
        _root = root.Q<VisualElement>("NumOfRoundsScreen");
        Button8Rounds = _root.Q<Button>("8-rounds-button");
        Button12Rounds = _root.Q<Button>("12-rounds-button");
        Button16Rounds = _root.Q<Button>("16-rounds-button");
        ConfirmButton = _root.Q<Button>("rounds-button-confirm");
    }

    public void Hide() => _root.style.display = DisplayStyle.None;
    public void Show() => _root.style.display = DisplayStyle.Flex;
}


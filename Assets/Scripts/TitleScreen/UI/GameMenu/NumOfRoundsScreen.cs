using UnityEngine.UIElements;

public readonly struct ButtonData
{
    public readonly string ClassName; 
    public readonly int Value; 
    public ButtonData(string className, int value)
    {
        ClassName = className;
        Value = value;
    }
}

public static class NumOfRoundsButtonData 
{
    public static readonly ButtonData[] Buttons = 
    {
        new("8-rounds-button", 8),
        new("12-rounds-button", 12),
        new("16-rounds-button", 16),
    };
}

public class NumOfRoundsScreen : IOptionsScreen
{
    private readonly VisualElement _root;
    public SelectableButtonElement[] RoundsButtonElements = new SelectableButtonElement[NumOfRoundsButtonData.Buttons.Length];
    public Button ConfirmButton { get; private set; }
    Button IOptionsScreen.ConfirmButton { get => ConfirmButton; set => ConfirmButton = value; }

    public NumOfRoundsScreen(VisualElement root)
    {
        _root = root.Q<VisualElement>("NumOfRoundsScreen");
        int idx = 0;
        foreach (var buttonData in NumOfRoundsButtonData.Buttons)
        {
            RoundsButtonElements[idx] = new SelectableButtonElement(_root.Q<Button>(buttonData.ClassName), buttonData.Value);
            RoundsButtonElements[idx].DisableBorder();
            idx++;
        }
        ConfirmButton = _root.Q<Button>("rounds-button-confirm");
    }

    public void Hide() => _root.style.display = DisplayStyle.None;
    public void Show() => _root.style.display = DisplayStyle.Flex;

    public SelectableButtonElement[] GetSelectableButtonElements() => RoundsButtonElements;
}


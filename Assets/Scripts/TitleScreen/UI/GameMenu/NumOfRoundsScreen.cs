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
    public static ButtonData[] Buttons = 
    {
        new("8-rounds-button", 8),
        new("12-rounds-button", 12),
        new("16-rounds-button", 16),
    };
}

public class NumOfRoundsButtonElement
{
    private const string BorderClassName = "dbr-btn-picked";
    public readonly VisualElement Border; 
    public readonly Button Button; 
    public readonly int Value;
    public NumOfRoundsButtonElement(Button button, int value)
    {
        Button = button;
        Border = Button.parent;
        Value = value;
    }

    public void EnableBorder() => Border?.EnableInClassList(BorderClassName, true); 
    public void DisableBorder() => Border?.EnableInClassList(BorderClassName, false);
}

public class NumOfRoundsScreen
{
    private readonly VisualElement _root;
    public NumOfRoundsButtonElement[] RoundsButtonElements = new NumOfRoundsButtonElement[NumOfRoundsButtonData.Buttons.Length];
    public Button ConfirmButton { get; private set; }

    public NumOfRoundsScreen(VisualElement root)
    {
        _root = root.Q<VisualElement>("NumOfRoundsScreen");
        int idx = 0;
        foreach (var buttonData in NumOfRoundsButtonData.Buttons)
        {
            RoundsButtonElements[idx] = new NumOfRoundsButtonElement(_root.Q<Button>(buttonData.ClassName), buttonData.Value);
            idx++;
        }
        ConfirmButton = _root.Q<Button>("rounds-button-confirm");
    }

    public void Hide() => _root.style.display = DisplayStyle.None;
    public void Show() => _root.style.display = DisplayStyle.Flex;
}


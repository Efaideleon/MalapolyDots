using UnityEngine.UIElements;

namespace UI.GameMenu
{
    public readonly struct ButtonPlayerData
    {
        public readonly string ClassName; 
        public readonly int Value; 
        public ButtonPlayerData(string className, int value)
        {
            ClassName = className;
            Value = value;
        }
    }

    public static class NumberOfPlayersScreenData
    {
        public static readonly ButtonPlayerData[] Buttons = 
        {
            new("num-of-players_2-button", 2),
            new("num-of-players_3-button", 3),
            new("num-of-players_4-button", 4),
            new("num-of-players_5-button", 5),
            new("num-of-players_6-button", 6),
        };
    }

    public class NumberOfPlayersScreen: IOptionsScreen
    {
        private readonly VisualElement _root;

        public SelectableButtonElement[] ButtonPlayerElements { get; private set; }
        public Button ConfirmButton { get; private set; }
        Button IOptionsScreen.ConfirmButton { get => ConfirmButton; set => ConfirmButton = value; }

        public NumberOfPlayersScreen(VisualElement root)
        {
            _root = root.Q<VisualElement>("NumOfPlayerScreen");

            ButtonPlayerElements = new SelectableButtonElement[NumberOfPlayersScreenData.Buttons.Length];

            int idx = 0;
            foreach (var buttonData in NumberOfPlayersScreenData.Buttons)
            {
                var button = _root.Q<Button>(buttonData.ClassName);
                ButtonPlayerElements[idx] = new(button, buttonData.Value);
                ButtonPlayerElements[idx].DisableBorder();
                idx++;
            }

            ConfirmButton = _root.Q<Button>("num-of-players_confirm-button");
        }
        
        public void Hide() => _root.style.display = DisplayStyle.None;
        public void Show() => _root.style.display = DisplayStyle.Flex;

        public SelectableButtonElement[] GetSelectableButtonElements() => ButtonPlayerElements;
    }
}

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
        public static ButtonPlayerData[] Buttons = 
        {
            new("num-of-players_2-button", 2),
            new("num-of-players_3-button", 3),
            new("num-of-players_4-button", 4),
            new("num-of-players_5-button", 5),
            new("num-of-players_6-button", 6),
        };
    }

    public class ButtonPlayerElement
    {
        public const string BorderClassName = "dbr-btn-picked";
        public readonly VisualElement Border; 
        public readonly Button Button;
        public readonly int Value; 

        public ButtonPlayerElement(Button button, VisualElement border,  int value)
        {
            Border = border;
            Button = button;
            Value = value;
        }

        public void EnableBorder() => Border?.EnableInClassList(BorderClassName, true); 
        public void DisableBorder() => Border?.EnableInClassList(BorderClassName, false);
    }

    public class NumberOfPlayersScreen
    {
        private readonly VisualElement _root;

        public ButtonPlayerElement[] ButtonPlayerElements { get; private set; }
        public Button ConfirmButton { get; private set; }
        
        public NumberOfPlayersScreen(VisualElement root)
        {
            _root = root.Q<VisualElement>("NumOfPlayerScreen");

            ButtonPlayerElements = new ButtonPlayerElement[NumberOfPlayersScreenData.Buttons.Length];

            int idx = 0;
            foreach (var buttonData in NumberOfPlayersScreenData.Buttons)
            {
                var button = _root.Q<Button>(buttonData.ClassName);
                ButtonPlayerElements[idx] = new(button, button.parent, buttonData.Value);
                ButtonPlayerElements[idx].DisableBorder();
                idx++;
            }

            ConfirmButton = _root.Q<Button>("num-of-players_confirm-button");
        }
        
        public void Hide() => _root.style.display = DisplayStyle.None;
        public void Show() => _root.style.display = DisplayStyle.Flex;
    }
}

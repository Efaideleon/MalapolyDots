using UnityEngine.UIElements;

namespace UI.GameMenu
{
    public class NumberOfPlayersScreen
    {
        private readonly VisualElement _root;
        public static string[] classNames = 
        {
            "num-of-players_2-button",
            "num-of-players_3-button",
            "num-of-players_4-button",
            "num-of-players_5-button",
            "num-of-players_6-button"
        };
        public readonly Button Button2Players;
        public readonly Button Button3Players;
        public readonly Button Button4Players;
        public readonly Button Button5Players;
        public readonly Button Button6Players;

        public VisualElement[] ButtonsContainer { get; private set; }
        public Button[] Buttons { get; private set; }
        public Button ConfirmButton { get; private set; }
        public const string BorderClassName = "dbr-btn-picked";
        
        public NumberOfPlayersScreen(VisualElement root)
        {
            _root = root.Q<VisualElement>("NumOfPlayerScreen");

            Buttons = new Button[classNames.Length];
            for (int i = 0; i < classNames.Length; i++)
                Buttons[i] = _root.Q<Button>(classNames[i]);

            ButtonsContainer =  new VisualElement[Buttons.Length];
            for (int i = 0; i < Buttons.Length; i++)
            {
                ButtonsContainer[i] = Buttons[i].parent;
                ButtonsContainer[i].EnableInClassList(BorderClassName, false);
            }

            ConfirmButton = _root.Q<Button>("num-of-players_confirm-button");
        }
        
        public void Hide() => _root.style.display = DisplayStyle.None;
        public void Show() => _root.style.display = DisplayStyle.Flex;
    }
}

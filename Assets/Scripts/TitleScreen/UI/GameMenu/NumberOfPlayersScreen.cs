using UnityEngine.UIElements;

namespace UI.GameMenu
{
    public class NumberOfPlayersScreen
    {
        private readonly VisualElement _root;
        public readonly Button Button2Players;
        public readonly Button Button3Players;
        public readonly Button Button4Players;
        public readonly Button Button5Players;
        public readonly Button Button6Players;
        public Button ConfirmButton { get; private set; }
        
        public NumberOfPlayersScreen(VisualElement root)
        {
            _root = root.Q<VisualElement>("NumOfPlayerScreen");
            Button2Players = _root.Q<Button>("num-of-players_2-button");
            Button3Players = _root.Q<Button>("num-of-players_3-button");
            Button4Players = _root.Q<Button>("num-of-players_4-button");
            Button5Players = _root.Q<Button>("num-of-players_5-button");
            Button6Players = _root.Q<Button>("num-of-players_6-button");
            ConfirmButton = _root.Q<Button>("num-of-players_confirm-button");
        }
        
        public void Hide() => _root.style.display = DisplayStyle.None;
        public void Show() => _root.style.display = DisplayStyle.Flex;
    }
}

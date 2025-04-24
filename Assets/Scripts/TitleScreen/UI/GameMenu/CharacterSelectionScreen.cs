using UnityEngine.UIElements;

public class CharacterSelectionScreen
{
        private readonly VisualElement _root;
        public readonly Button AvocadoButton;
        public readonly Button BirdButton;
        public readonly Button CoinButton;
        public readonly Button LiraButton;
        public readonly Button MugButton;
        public readonly Button TuctucButton;
        public Button ConfirmButton { get; private set; }

        public CharacterSelectionScreen(VisualElement root)
        {
            _root = root.Q<VisualElement>("CharacterSelectScreen");
            AvocadoButton = _root.Q<Button>("character-one-button");
            BirdButton = _root.Q<Button>("character-two-button");
            CoinButton = _root.Q<Button>("character-three-button");
            LiraButton = _root.Q<Button>("character-four-button");
            MugButton = _root.Q<Button>("chracter-five-button");
            TuctucButton = _root.Q<Button>("character-six-button");
            ConfirmButton = _root.Q<Button>("character-confirm-button");
        }

        public void Hide() => _root.style.display = DisplayStyle.None;
        public void Show() => _root.style.display = DisplayStyle.Flex;
}

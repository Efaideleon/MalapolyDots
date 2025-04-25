using Unity.Collections;
using UnityEngine.UIElements;

public class CharacterSelectionScreen
{
        private readonly VisualElement _root;
        public Button[] CharButtons = new Button[6];
        private readonly string[] _characterButtonsClassNames = 
        {
            "character-one-button",
            "character-two-button",
            "character-three-button",
            "character-four-button",
            "character-five-button",
            "character-six-button"
        };
        public readonly Label PlayerNumberLabel;
        public Button ConfirmButton { get; private set; }

        public CharacterSelectionScreen(VisualElement root)
        {
            _root = root.Q<VisualElement>("CharacterSelectScreen");
            for (int i = 0; i < _characterButtonsClassNames.Length; i++)
            {
                CharButtons[i] = _root.Q<Button>(_characterButtonsClassNames[i]);
            }
            PlayerNumberLabel = _root.Q<Label>("player-number-label");
            ConfirmButton = _root.Q<Button>("character-confirm-button");
        }

        public void Hide() => _root.style.display = DisplayStyle.None;
        public void Show() => _root.style.display = DisplayStyle.Flex;
}

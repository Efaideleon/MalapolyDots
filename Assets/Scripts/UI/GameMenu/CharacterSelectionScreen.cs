using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;


class CharacterSelectionScreen
{
        private const string AvocadoName = "Avocado";
        private const string BirdName = "Bird";
        private const string CoinName = "Coin";
        private const string LiraName = "Lira";
        private const string MugName = "Mug";
        private const string TucTucName = "TucTuc";
        
        private readonly VisualElement _root;
        private readonly Button _avocado;
        private readonly Button _bird;
        private readonly Button _coin;
        private readonly Button _lira;
        private readonly Button _mug;
        private readonly Button _tuctuc;
        public Button ConfirmButton { get; private set; }
        private readonly GameData _gameData;
        private string _selectedCharacter;

        public CharacterSelectionScreen(VisualElement root, GameData gameData)
        {
            _gameData = gameData;
            _root = root.Q<VisualElement>("CharacterSelectScreen");
            _avocado = _root.Q<Button>("character-one-button");
            _bird = _root.Q<Button>("character-two-button");
            _coin = _root.Q<Button>("character-three-button");
            _lira = _root.Q<Button>("character-four-button");
            _mug = _root.Q<Button>("chracter-five-button");
            _tuctuc = _root.Q<Button>("character-six-button");
            ConfirmButton = _root.Q<Button>("character-confirm-button");

            _avocado.clicked += OnButton2Clicked;
            _bird.clicked += OnButton3Clicked;
            _coin.clicked += OnButton4Clicked;
            _lira.clicked += OnButton5Clicked;
            _mug.clicked += OnButton6Clicked;
            _tuctuc.clicked += OnButton7Clicked;
            ConfirmButton.clicked += OnConfirmClicked;
        }

        private void OnButton2Clicked() => SetObjectPrefab(AvocadoName);
        private void OnButton3Clicked() => SetObjectPrefab(BirdName);
        private void OnButton4Clicked() => SetObjectPrefab(CoinName);
        private void OnButton5Clicked() => SetObjectPrefab(LiraName);
        private void OnButton6Clicked() => SetObjectPrefab(MugName);
        private void OnButton7Clicked() => SetObjectPrefab(TucTucName);

        private void SetObjectPrefab(string characterName)
        {
            _selectedCharacter = characterName;
        }
        
        private void OnConfirmClicked()
        {
            _gameData.charactersSelected.Add(_selectedCharacter);
        }

        public void OnDispose()
        {
            _avocado.clicked -= OnButton2Clicked;
            _bird.clicked -= OnButton3Clicked;
            _coin.clicked -= OnButton4Clicked;
            _lira.clicked -= OnButton5Clicked;
            _mug.clicked -= OnButton6Clicked;
            _tuctuc.clicked -= OnButton7Clicked;
        }

        public void Hide() => _root.style.display = DisplayStyle.None;
        public void Show() => _root.style.display = DisplayStyle.Flex;
}

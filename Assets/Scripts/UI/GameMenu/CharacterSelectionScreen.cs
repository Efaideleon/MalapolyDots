using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;


class CharacterSelectionScreen
{
        private readonly VisualElement _root;
        private readonly Button _avocado;
        private readonly Button _bird;
        private readonly Button _coin;
        private readonly Button _lira;
        private readonly Button _mug;
        private readonly Button _tuctuc;
        public Button ConfirmButton { get; private set; }
        private readonly GameData _gameData;

        public CharacterSelectionScreen(VisualElement root, GameData gameData)
        {
            _root = root.Q<VisualElement>("CharacterSelectScreen");
            _avocado = _root.Q<Button>("character-one-button");
            _bird = _root.Q<Button>("character-two-button");
            _coin = _root.Q<Button>("character-three-button");
            _lira = _root.Q<Button>("character-four-button");
            _mug = _root.Q<Button>("character-five-button");
            _tuctuc = _root.Q<Button>("character-six-button");


            ConfirmButton = _root.Q<Button>("character-confirm-button");
            _gameData = gameData;

            _avocado.clicked += OnButton2Clicked;
            _bird.clicked += OnButton3Clicked;
            _coin.clicked += OnButton4Clicked;
            _lira.clicked += OnButton5Clicked;
            _mug.clicked += OnButton6Clicked;
            _tuctuc.clicked += OnButton7Clicked;

        }

        private void OnButton2Clicked() => SetObjectPrefab(2);
        private void OnButton3Clicked() => SetObjectPrefab(3);
        private void OnButton4Clicked() => SetObjectPrefab(4);
        private void OnButton5Clicked() => SetObjectPrefab(5);
        private void OnButton6Clicked() => SetObjectPrefab(6);
        private void OnButton7Clicked() => SetObjectPrefab(6);

        private void SetObjectPrefab(int numOfPlayers)
        {
            _gameData.numberOfPlayers = numOfPlayers;
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

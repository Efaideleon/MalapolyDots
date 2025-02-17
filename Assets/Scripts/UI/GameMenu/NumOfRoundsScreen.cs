using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

class NumOfRoundsScreen
{
    private readonly VisualElement _root;
    private readonly Button _button2Players;
    private readonly Button _button3Players;
    private readonly Button _button4Players;
    private readonly Button _button5Players;
    private readonly Button _button6Players;
    public Button ConfirmButton { get; private set; }
    private readonly GameData _gameData;

    public NumOfRoundsScreen(VisualElement root, GameData gameData)
    {
        _root = root.Q<VisualElement>("NumOfPlayerScreen");
        _button2Players = _root.Q<Button>("num-of-players_2-button");
        _button3Players = _root.Q<Button>("num-of-players_3-button");
        _button4Players = _root.Q<Button>("num-of-players_4-button");
        _button5Players = _root.Q<Button>("num-of-players_5-button");
        _button6Players = _root.Q<Button>("num-of-players_6-button");
        ConfirmButton = _root.Q<Button>("num-of-players_confirm-button");
        _gameData = gameData;

        _button2Players.clicked += OnButton2Clicked;
        _button3Players.clicked += OnButton3Clicked;
        _button4Players.clicked += OnButton4Clicked;
        _button5Players.clicked += OnButton5Clicked;
        _button6Players.clicked += OnButton6Clicked;
    }

    private void OnButton2Clicked() => SetNumberOfPlayers(2);
    private void OnButton3Clicked() => SetNumberOfPlayers(3);
    private void OnButton4Clicked() => SetNumberOfPlayers(4);
    private void OnButton5Clicked() => SetNumberOfPlayers(5);
    private void OnButton6Clicked() => SetNumberOfPlayers(6);

    private void SetNumberOfPlayers(int numOfPlayers)
    {
        _gameData.numberOfPlayers = numOfPlayers;
    }

    public void OnDispose()
    {
        _button2Players.clicked -= OnButton2Clicked;
        _button3Players.clicked -= OnButton3Clicked;
        _button4Players.clicked -= OnButton4Clicked;
        _button5Players.clicked -= OnButton5Clicked;
        _button6Players.clicked -= OnButton6Clicked;
    }

    public void Hide() => _root.style.display = DisplayStyle.None;
    public void Show() => _root.style.display = DisplayStyle.Flex;
}


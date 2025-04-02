using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

class NumOfRoundsScreen
{
    private readonly VisualElement _root;
    private readonly Button _button8Rounds;
    private readonly Button _button12Rounds;
    private readonly Button _button16Rounds;
    public Button ConfirmButton { get; private set; }
    private readonly GameData _gameData;

    public NumOfRoundsScreen(VisualElement root, GameData gameData)
    {
        _root = root.Q<VisualElement>("NumOfRoundsScreen");
        _button8Rounds = _root.Q<Button>("8-rounds-button");
        _button12Rounds = _root.Q<Button>("12-rounds-button");
        _button16Rounds = _root.Q<Button>("16-rounds-button");
        ConfirmButton = _root.Q<Button>("rounds-button-confirm");
        _gameData = gameData;

        _button8Rounds.clicked += OnButton2Clicked;
        _button12Rounds.clicked += OnButton3Clicked;
        _button16Rounds.clicked += OnButton4Clicked;
    }

    private void OnButton2Clicked() => SetNumberOfRounds(8);
    private void OnButton3Clicked() => SetNumberOfRounds(12);
    private void OnButton4Clicked() => SetNumberOfRounds(16);
    private void SetNumberOfRounds(int numOfRounds)
    {
        _gameData.numberOfRounds = numOfRounds;
    }

    public void OnDispose()
    {
        _button8Rounds.clicked -= OnButton2Clicked;
        _button12Rounds.clicked -= OnButton3Clicked;
        _button16Rounds.clicked -= OnButton4Clicked;
    }

    public void Hide() => _root.style.display = DisplayStyle.None;
    public void Show() => _root.style.display = DisplayStyle.Flex;
}


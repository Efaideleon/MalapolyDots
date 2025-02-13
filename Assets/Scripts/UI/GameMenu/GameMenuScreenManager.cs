using UnityEngine;
using UnityEngine.UIElements;

public class GameMenuScreenManager : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    private VisualElement _root;
    private VisualElement _titleScreen;
    private VisualElement _numOfRoundsScreen;
    private VisualElement _numOfPlayersScreen;
    private VisualElement _characterSelectScreen;

    private Button _titleScreenPlayButton;
    private Button _numOfRoundsScreenConfirmButton;
    private Button _numOfPlayersScreenConfirmButton;
    private Button _characterSelectConfirmButton;

    void Start()
    {
        _root = _uiDocument.rootVisualElement.Q<VisualElement>("game-menu-root");
        _titleScreen = _root.Q<VisualElement>("TitleScreen");
        _numOfRoundsScreen = _root.Q<VisualElement>("NumOfRoundsScreen");
        _numOfPlayersScreen = _root.Q<VisualElement>("NumOfPlayerScreen");
        _characterSelectScreen = _root.Q<VisualElement>("CharacterSelectScreen");

        _titleScreenPlayButton = _titleScreen.Q<Button>("start-button");
        _numOfRoundsScreenConfirmButton = _numOfRoundsScreen.Q<Button>("rounds-button-confirm");
        _numOfPlayersScreenConfirmButton = _numOfPlayersScreen.Q<Button>("num-of-players_confirm-button");
        _characterSelectConfirmButton = _characterSelectScreen.Q<Button>("character-confirm");

        // Registering to button click events that change the screen
        _titleScreenPlayButton.clicked += EnableNumOfPlayersScreen;
        _numOfPlayersScreenConfirmButton.clicked += EnableCharacterSelectScreen;
        _characterSelectConfirmButton.clicked += EnableNumOfRoundsScreen;

        EnableTitleScreen();
    }

    private void EnableTitleScreen()
    {
        _titleScreen.style.display = DisplayStyle.Flex;
        _numOfRoundsScreen.style.display = DisplayStyle.None;
        _numOfPlayersScreen.style.display = DisplayStyle.None;
        _characterSelectScreen.style.display = DisplayStyle.None;
    }

    private void EnableNumOfRoundsScreen()
    {
        _titleScreen.style.display = DisplayStyle.None;
        _numOfRoundsScreen.style.display = DisplayStyle.Flex;
        _numOfPlayersScreen.style.display = DisplayStyle.None;
        _characterSelectScreen.style.display = DisplayStyle.None;
    }

    private void EnableNumOfPlayersScreen()
    {
        _titleScreen.style.display = DisplayStyle.None;
        _numOfRoundsScreen.style.display = DisplayStyle.None;
        _numOfPlayersScreen.style.display = DisplayStyle.Flex;
        _characterSelectScreen.style.display = DisplayStyle.None;
    }

    private void EnableCharacterSelectScreen()
    {
        _titleScreen.style.display = DisplayStyle.None;
        _numOfRoundsScreen.style.display = DisplayStyle.None;
        _numOfPlayersScreen.style.display = DisplayStyle.None;
        _characterSelectScreen.style.display = DisplayStyle.Flex;
    }

    void OnDisable()
    {
        _titleScreenPlayButton.clicked -= EnableNumOfPlayersScreen;
        _numOfPlayersScreenConfirmButton.clicked -= EnableCharacterSelectScreen;
        _characterSelectConfirmButton.clicked -= EnableNumOfRoundsScreen;
    }
}

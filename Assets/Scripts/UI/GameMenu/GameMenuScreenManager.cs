using UI.GameMenu;
using UnityEngine;
using UnityEngine.UIElements;

public class GameMenuScreenManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private SceneLoadersSystem sceneLoaderSystem;
    [SerializeField] private GameData gameData;
    private VisualElement _root;

    private NumberOfPlayersScreen _numOfPlayersScreen;
    private CharacterSelectionScreen _characterSelectionScreen;
    private TitleScreen _titleScreen;
    
    private VisualElement _numOfRoundsScreen;

    private Button _titleScreenPlayButton;
    private Button _numOfRoundsScreenConfirmButton;
    private Button _numOfPlayersScreenConfirmButton;
    private Button _characterSelectConfirmButton;

    void Start()
    {
        _root = uiDocument.rootVisualElement.Q<VisualElement>("game-menu-root");
        _numOfPlayersScreen = new NumberOfPlayersScreen(_root, gameData);
        
        _titleScreen = _root.Q<VisualElement>("TitleScreen");
        _numOfRoundsScreen = _root.Q<VisualElement>("NumOfRoundsScreen");
        _characterSelectScreen = _root.Q<VisualElement>("CharacterSelectScreen");

        _titleScreenPlayButton = _titleScreen.Q<Button>("start-button");
        _numOfRoundsScreenConfirmButton = _numOfRoundsScreen.Q<Button>("rounds-button-confirm");
        _characterSelectConfirmButton = _characterSelectScreen.Q<Button>("character-confirm");

        // Registering to button click events that change the screen
        _titleScreenPlayButton.clicked += EnableNumOfPlayersScreen;
        _numOfPlayersScreen.ConfirmButton.clicked += EnableCharacterSelectScreen;
        _characterSelectConfirmButton.clicked += EnableNumOfRoundsScreen;
        _numOfRoundsScreenConfirmButton.clicked += GoToGameScene;
        EnableTitleScreen();
    }

    private void EnableTitleScreen()
    {
        _titleScreen.style.display = DisplayStyle.Flex;
        _numOfRoundsScreen.style.display = DisplayStyle.None;
        _numOfPlayersScreen.Hide();
        _characterSelectScreen.style.display = DisplayStyle.None;
    }

    private void EnableNumOfRoundsScreen()
    {
        _titleScreen.style.display = DisplayStyle.None;
        _numOfRoundsScreen.style.display = DisplayStyle.Flex;
        _numOfPlayersScreen.Hide();
        _characterSelectScreen.style.display = DisplayStyle.None;
    }

    private void EnableNumOfPlayersScreen()
    {
        _titleScreen.style.display = DisplayStyle.None;
        _numOfRoundsScreen.style.display = DisplayStyle.None;
        _numOfPlayersScreen.Show();
        _characterSelectScreen.style.display = DisplayStyle.None;
    }

    private void EnableCharacterSelectScreen()
    {
        _titleScreen.style.display = DisplayStyle.None;
        _numOfRoundsScreen.style.display = DisplayStyle.None;
        _numOfPlayersScreen.Hide();
        _characterSelectScreen.style.display = DisplayStyle.Flex;
    }

    private void GoToGameScene()
    {
        sceneLoaderSystem.LoadSceneBySceneIDEnum(SceneID.Game);
    }

    void OnDisable()
    {
        _titleScreenPlayButton.clicked -= EnableNumOfPlayersScreen;
        _numOfPlayersScreen.ConfirmButton.clicked -= EnableNumOfRoundsScreen;
        _characterSelectConfirmButton.clicked -= EnableNumOfRoundsScreen;
        _numOfPlayersScreen.OnDispose();
    }
}

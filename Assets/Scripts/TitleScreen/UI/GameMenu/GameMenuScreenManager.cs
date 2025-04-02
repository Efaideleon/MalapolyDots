using UI.GameMenu;
using UnityEngine;
using UnityEngine.UIElements;

public class GameMenuScreenManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private SceneLoadersSystem sceneLoaderSystem;
    [SerializeField] private GameData gameData;
    private VisualElement _root;
    private const string GameMenuRoot = "game-menu-root";

    private NumberOfPlayersScreen _numOfPlayersScreen;
    private CharacterSelectionScreen _characterSelectionScreen;
    private TitleScreen _titleScreen;
    private NumOfRoundsScreen _numOfRoundsScreen;

    void Start()
    {
        gameData.Reset();
        _root = uiDocument.rootVisualElement.Q<VisualElement>(GameMenuRoot);
        InitializeScreens(_root);
        SubscribeToEvents();
        EnableTitleScreen();
    }

    private void SubscribeToEvents()
    {
        _titleScreen.StartButton.clicked += EnableNumOfPlayersScreen;
        _numOfPlayersScreen.ConfirmButton.clicked += EnableCharacterSelectScreen;
        _characterSelectionScreen.OnAllCharactersSelected += EnableNumOfRoundsScreen;
        _numOfRoundsScreen.ConfirmButton.clicked += GoToGameScene;
    }

    private void InitializeScreens(VisualElement root)
    {
        _titleScreen = new TitleScreen(root);
        _numOfRoundsScreen = new NumOfRoundsScreen(root, gameData);
        _numOfPlayersScreen = new NumberOfPlayersScreen(root, gameData);
        _characterSelectionScreen = new CharacterSelectionScreen(root, gameData);
    }

    private void EnableTitleScreen()
    {
        _titleScreen.Show();
        _numOfRoundsScreen.Hide();
        _numOfPlayersScreen.Hide();
        _characterSelectionScreen.Hide();
    }

    private void EnableNumOfRoundsScreen()
    {
        _titleScreen.Hide();
        _numOfRoundsScreen.Show();
        _numOfPlayersScreen.Hide();
        _characterSelectionScreen.Hide();
    }

    private void EnableNumOfPlayersScreen()
    {
        _titleScreen.Hide();
        _numOfRoundsScreen.Hide();
        _numOfPlayersScreen.Show();
        _characterSelectionScreen.Hide();
    }

    private void EnableCharacterSelectScreen()
    {
        _titleScreen.Hide();
        _numOfRoundsScreen.Hide();
        _numOfPlayersScreen.Hide();
        _characterSelectionScreen.Show();
    }

    private void GoToGameScene()
    {
        sceneLoaderSystem.LoadSceneBySceneIDEnum(SceneID.Game);
    }

    void OnDisable()
    {
        _titleScreen.StartButton.clicked -= EnableNumOfPlayersScreen;
        _numOfPlayersScreen.ConfirmButton.clicked -= EnableNumOfRoundsScreen;
        _characterSelectionScreen.OnAllCharactersSelected -= EnableNumOfRoundsScreen;
        _numOfPlayersScreen.OnDispose();
    }
}

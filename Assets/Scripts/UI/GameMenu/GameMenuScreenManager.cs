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
    private Button _numOfRoundsScreenConfirmButton;

    void Start()
    {
        _root = uiDocument.rootVisualElement.Q<VisualElement>("game-menu-root");
        _numOfPlayersScreen = new NumberOfPlayersScreen(_root, gameData);
        _titleScreen = new TitleScreen(_root); 
        _characterSelectionScreen = new CharacterSelectionScreen(_root, gameData);
        
        _numOfRoundsScreen = _root.Q<VisualElement>("NumOfRoundsScreen");
        _numOfRoundsScreenConfirmButton = _numOfRoundsScreen.Q<Button>("rounds-button-confirm");

        // Registering to button click events that change the screen
        _titleScreen.StartButton.clicked += EnableNumOfPlayersScreen;
        _numOfPlayersScreen.ConfirmButton.clicked += EnableCharacterSelectScreen;
        _characterSelectionScreen.ConfirmButton.clicked += EnableNumOfRoundsScreen;
        
        _numOfRoundsScreenConfirmButton.clicked += GoToGameScene;
        EnableTitleScreen();
    }

    private void EnableTitleScreen()
    {
        _titleScreen.Show();
        _numOfRoundsScreen.style.display = DisplayStyle.None;
        _numOfPlayersScreen.Hide();
        _characterSelectionScreen.Hide();
    }

    private void EnableNumOfRoundsScreen()
    {
        _titleScreen.Hide();
        _numOfRoundsScreen.style.display = DisplayStyle.Flex;
        _numOfPlayersScreen.Hide();
        _characterSelectionScreen.Hide();
    }

    private void EnableNumOfPlayersScreen()
    {
        _titleScreen.Hide();
        _numOfRoundsScreen.style.display = DisplayStyle.None;
        _numOfPlayersScreen.Show();
        _characterSelectionScreen.Hide();
    }

    private void EnableCharacterSelectScreen()
    {
        _titleScreen.Hide();
        _numOfRoundsScreen.style.display = DisplayStyle.None;
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
        _characterSelectionScreen.ConfirmButton.clicked -= EnableNumOfRoundsScreen;
        _numOfPlayersScreen.OnDispose();
    }
}

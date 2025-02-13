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
        _numOfPlayersScreen = _root.Q<VisualElement>("NumOfPlayersScreen");
        _characterSelectScreen = _root.Q<VisualElement>("CharacterSelectScreen");

        _titleScreenPlayButton = _titleScreenPlayButton.Q<Button>("start-button");
        /*_numOfRoundsScreenConfirmButton = _numOfRoundsScreen.Q<Button>()*/
        _numOfPlayersScreenConfirmButton = _numOfPlayersScreen.Q<Button>("num-of-players_confirm-button");
        _characterSelectConfirmButton = _characterSelectScreen.Q<Button>();
    }
}

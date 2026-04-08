using Unity.Entities;

namespace TitleScreen.NetworkUI.Components
{
    public struct UIEvent : IComponentData
    { }

    public struct MainMenuHostClickEvent : IComponentData
    { }

    public struct MainMenuJoinClickEvent : IComponentData
    { }

    public struct HostSetupHostClickEvent : IComponentData
    { }

    public struct JoinSetupJoinClickEvent : IComponentData
    { }

    public struct LobbyStartClickEvent : IComponentData
    { }

    public struct ExitConnectionClickEvent : IComponentData
    { }

    public struct BackToMainMenuClickEvent : IComponentData
    { }


    // Character Selection Buttons
    public struct AvocadoClickEvent : IComponentData
    { }

    public struct BirdClickEvent : IComponentData
    { }

    public struct CoinClickEvent : IComponentData
    { }

    public struct LiraClickEvent : IComponentData
    { }

    public struct CoffeeClickEvent : IComponentData
    { }

    public struct TuctucClickEvent : IComponentData
    { }

    public struct CharacterSelectConfirmClickEvent : IComponentData
    { }

    public struct GameMenuPhaseComponent : IComponentData
    {
        public GameMenuPhase Value;
    }

    public enum GameMenuPhase
    {
        MainMenu,
        HostSetup,
        JoinSetup,
        Lobby,
        CharacterSelect
    }
}

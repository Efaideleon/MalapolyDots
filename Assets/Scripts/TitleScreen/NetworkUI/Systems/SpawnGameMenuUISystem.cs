using System.Collections.Generic;
using Assets.Scripts.DOTS.DataComponents;
using DOTS.DataComponents;
using TitleScreen.NetworkUI.Authoring;
using TitleScreen.NetworkUI.Components;
using TitleScreen.NetworkUI.Panels;
using Unity.Entities;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SpawnGameMenuUISystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkGameMenuReference>();
            state.EntityManager.CreateSingleton(new GameMenuPhaseComponent { Value = GameMenuPhase.MainMenu });
            state.EntityManager.CreateSingleton(new NetworkRoleTypeComponent { Value = NetworkRole.None });
        }

        public void OnStartRunning(ref SystemState state)
        {
            var gameMenuRef = SystemAPI.ManagedAPI.GetSingleton<NetworkGameMenuReference>();
            var gameMenuGO = gameMenuRef.uiDocumentGO;
            if (gameMenuGO == null)
                return;

            UnityEngine.Debug.Log($"[SpawnGameMenuUISystem] | spawning ui doc");
            // Create the ui toolkit game menu.
            var uiGameObject = UnityEngine.Object.Instantiate(gameMenuRef.uiDocumentGO);
            if (!uiGameObject.TryGetComponent<UIDocument>(out var uiDocument))
            {
                UnityEngine.Object.Destroy(uiGameObject);
                return;
            }

            var entityReference = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<GameObjectReference>(entityReference);
            state.EntityManager.AddComponentData(entityReference, new GameObjectReference { Instance = uiGameObject });
            state.EntityManager.AddComponent<GameMenuTag>(entityReference);

            var gameMenuRoot = uiDocument.rootVisualElement;

            // Panels creation.
            var entity = state.EntityManager.CreateEntity();
            var panelsComponent = new GameMenuPanelsComponent
            {
                AllPanels = new(),
                PanelLookup = new()
            };
            state.EntityManager.AddComponentObject(entity, panelsComponent);

            // Adding GameMenuUIRequests queue to all panels.
            var uiRequestEntity = state.EntityManager.CreateEntity();
            var uiRequestsComponet = new GameMenuUIRequests
            {
                UIRequests = new()
            };
            state.EntityManager.AddComponentObject(uiRequestEntity, uiRequestsComponet);

            MainMenuPanel mainMenuPanel = new(gameMenuRoot.Q<VisualElement>("MainMenu"), uiRequestsComponet.UIRequests);
            HostSetupPanel hostSetupPanel = new(gameMenuRoot.Q<VisualElement>("HostSetup"), uiRequestsComponet.UIRequests);
            JoinSetupPanel joinSetupPanel = new(gameMenuRoot.Q<VisualElement>("JoinSetup"), uiRequestsComponet.UIRequests);
            LobbyPanel lobbyPanel = new(gameMenuRoot.Q<VisualElement>("Lobby"), uiRequestsComponet.UIRequests);
            CharacterSelectPanel characterSelectPanel = new(gameMenuRoot.Q<VisualElement>("CharacterSelect"), uiRequestsComponet.UIRequests);

            panelsComponent.AllPanels.Add(mainMenuPanel);
            panelsComponent.AllPanels.Add(hostSetupPanel);
            panelsComponent.AllPanels.Add(joinSetupPanel);
            panelsComponent.AllPanels.Add(lobbyPanel);
            panelsComponent.AllPanels.Add(characterSelectPanel);

            panelsComponent.PanelLookup[GameMenuPhase.MainMenu] = mainMenuPanel;
            panelsComponent.PanelLookup[GameMenuPhase.HostSetup] = hostSetupPanel;
            panelsComponent.PanelLookup[GameMenuPhase.JoinSetup] = joinSetupPanel;
            panelsComponent.PanelLookup[GameMenuPhase.Lobby] = lobbyPanel;
            panelsComponent.PanelLookup[GameMenuPhase.CharacterSelect] = characterSelectPanel;

            // Panel initialization
            foreach (var panel in panelsComponent.AllPanels)
            {
                panel.Initialize();
            }
        }

        public void OnStopRunning(ref SystemState state)
        {
            if (!SystemAPI.ManagedAPI.TryGetSingleton<GameMenuPanelsComponent>(out var gameMenuPanelsComponent))
                return;

            var gameMenuPanels = gameMenuPanelsComponent.AllPanels;
            if (gameMenuPanels != null)
            {
                foreach (var panel in gameMenuPanels)
                {
                    panel.Dispose();
                }
            }
        }

        public void OnDestroy(ref SystemState state)
        {

            if (!SystemAPI.ManagedAPI.TryGetSingleton<GameMenuPanelsComponent>(out var panels))
                return;

            var query = SystemAPI.QueryBuilder().WithAll<GameMenuPanelsComponent>().Build();
            state.EntityManager.DestroyEntity(query);
        }
    }

    public enum UIRequestType
    {
        MainMenuHost,
        MainMenuJoin,
        HostSetupHost,
        JoinSetupJoin,
        LobbyStartButton,
        AvocadoButton,
        BirdButtoon,
        CoinButton,
        LiraButton,
        CoffeButton,
        TuctucButton,
        CharacterSelectConfirmButton,
        Back
    }

    public struct UIRequest
    {
        public UIRequestType Value;
    }

    public class GameMenuUIRequests : IComponentData
    {
        public Queue<UIRequest> UIRequests;
    }

    // Does this component live on the default world?
    public class GameMenuPanelsComponent : IComponentData
    {
        public List<NetworkPanelBase> AllPanels;
        public Dictionary<GameMenuPhase, NetworkPanelBase> PanelLookup;
    }

    public struct GameMenuTag : IComponentData
    { }
}

using System.Collections.Generic;
using Unity.Collections;
using Assets.Scripts.DOTS.DataComponents;
using Assets.Scripts.TitleScreen.NetworkUI.Panels;
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
            Cleanup(ref state);
            SystemAPI.SetSingleton(new GameMenuPhaseComponent { Value = GameMenuPhase.MainMenu });
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
                Queue = new()
            };
            state.EntityManager.AddComponentObject(uiRequestEntity, uiRequestsComponet);

            MainMenuPanel mainMenuPanel = new(gameMenuRoot.Q<VisualElement>("MainMenu"), uiRequestsComponet.Queue);
            HostSetupPanel hostSetupPanel = new(gameMenuRoot.Q<VisualElement>("HostSetup"), uiRequestsComponet.Queue);
            JoinSetupPanel joinSetupPanel = new(gameMenuRoot.Q<VisualElement>("JoinSetup"), uiRequestsComponet.Queue);
            LobbyPanel lobbyPanel = new(gameMenuRoot.Q<VisualElement>("Lobby"), uiRequestsComponet.Queue);
            CharacterSelectPanel characterSelectPanel = new(gameMenuRoot.Q<VisualElement>("CharacterSelect"), uiRequestsComponet.Queue);
            SoloSetupPanel soloSetupPanel = new(gameMenuRoot.Q<VisualElement>("SoloSetup"), uiRequestsComponet.Queue);

            panelsComponent.AllPanels.Add(mainMenuPanel);
            panelsComponent.AllPanels.Add(hostSetupPanel);
            panelsComponent.AllPanels.Add(joinSetupPanel);
            panelsComponent.AllPanels.Add(lobbyPanel);
            panelsComponent.AllPanels.Add(characterSelectPanel);
            panelsComponent.AllPanels.Add(soloSetupPanel);

            panelsComponent.PanelLookup[GameMenuPhase.MainMenu] = mainMenuPanel;
            panelsComponent.PanelLookup[GameMenuPhase.HostSetup] = hostSetupPanel;
            panelsComponent.PanelLookup[GameMenuPhase.JoinSetup] = joinSetupPanel;
            panelsComponent.PanelLookup[GameMenuPhase.Lobby] = lobbyPanel;
            panelsComponent.PanelLookup[GameMenuPhase.CharacterSelect] = characterSelectPanel;
            panelsComponent.PanelLookup[GameMenuPhase.SoloSetup] = soloSetupPanel;

            // Panel initialization
            foreach (var panel in panelsComponent.AllPanels)
            {
                panel.Initialize();
                panel.Hide();
            }
            mainMenuPanel.Show();
        }

        public void OnStopRunning(ref SystemState state) => Cleanup(ref state);

        public void OnDestroy(ref SystemState state) => Cleanup(ref state);

        static void Cleanup(ref SystemState state)
        {
            var panelsQuery = state.GetEntityQuery(ComponentType.ReadOnly<GameMenuPanelsComponent>());
            using (var entities = panelsQuery.ToEntityArray(Allocator.Temp))
            {
                foreach (var entity in entities)
                {
                    var panels = state.EntityManager.GetComponentObject<GameMenuPanelsComponent>(entity);
                    if (panels.AllPanels == null) continue;
                    foreach (var panel in panels.AllPanels)
                    {
                        panel.Hide();
                        panel.Dispose();
                    }
                }
            }
            state.EntityManager.DestroyEntity(panelsQuery);
            state.EntityManager.DestroyEntity(state.GetEntityQuery(ComponentType.ReadOnly<GameMenuUIRequests>()));
            state.EntityManager.DestroyEntity(state.GetEntityQuery(ComponentType.ReadOnly<GameMenuTag>()));
        }
    }

    public enum UIRequestType
    {
        PlayButton,
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
        ExitConnection,
        BackToMainMenu,
        MainMenuSolo
    }

    public struct UIRequest
    {
        public UIRequestType Value;
    }

    public class GameMenuUIRequests : IComponentData
    {
        public Queue<UIRequest> Queue;
    }

    // Does this component live on the default world?
    public class GameMenuPanelsComponent : IComponentData
    {
        public List<NetworkPanelBase> AllPanels;
        public Dictionary<GameMenuPhase, NetworkPanelBase> PanelLookup;
        public MainMenuBackButton BackButton;
    }

    public struct GameMenuTag : IComponentData
    { }
}

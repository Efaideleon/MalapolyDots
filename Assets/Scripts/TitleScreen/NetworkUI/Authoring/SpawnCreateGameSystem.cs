using Assets.Scripts.DOTS.DataComponents;
using Assets.Common;
using TitleScreen.NetworkUI.Authoring;
using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SpawnCreateGameSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CreateGameUIReference>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            Cleanup(ref state);
            var uiRef = SystemAPI.ManagedAPI.GetSingleton<CreateGameUIReference>();
            var prefab = uiRef.uiDocumentGO;

            if (prefab == null)
                return;

            Debug.Log("[SpawnCreateGameSystem] Spawning CreateGame UI Document");
            var uiGameObject = UnityEngine.Object.Instantiate(prefab);
            if (!uiGameObject.TryGetComponent<UIDocument>(out var uiDocument))
            {
                UnityEngine.Object.Destroy(uiGameObject);
                return;
            }

            var root = uiDocument.rootVisualElement;
            var createGamePanel = new CreateGamePanel(root);

            // Create managed singleton component holding the panel instance.
            var panelEntity = state.EntityManager.CreateSingleton(new CreateGameUIPanelComponent { Panel = createGamePanel });
            state.EntityManager.AddComponentObject(panelEntity, new GameObjectReference { Instance = uiGameObject });
        }

        public void OnStopRunning(ref SystemState state) => Cleanup(ref state);

        public void OnDestroy(ref SystemState state) => Cleanup(ref state);

        static void Cleanup(ref SystemState state)
        {
            state.EntityManager.DestroyEntity(state.GetEntityQuery(ComponentType.ReadOnly<CreateGameUIPanelComponent>()));
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct HideCreateGamePanelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CreateGameUIPanelComponent>();
            state.RequireForUpdate<GameMenuPhaseComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var menuPhase = SystemAPI.GetSingleton<GameMenuPhaseComponent>();
            var panel = SystemAPI.ManagedAPI.GetSingleton<CreateGameUIPanelComponent>().Panel;
            if (panel == null)
                return;

            switch (menuPhase.Value)
            {
                // Adjust these cases to match the project's GameMenuPhase values.
                //case GameMenuPhase.HostSetup:
                case GameMenuPhase.HostSetup: // if you have a CreateGame phase
                    if (!panel.IsVisible) panel.Show();
                    break;

                default:
                    if (panel.IsVisible) panel.Hide();
                    break;
            }
        }
    }

    // Managed component used as a singleton container for the CreateGame panel.
    public class CreateGameUIPanelComponent : IComponentData
    {
        public CreateGamePanel Panel;
    }

    // Simple wrapper around the UI Document root for the Create Game UI.
    public class CreateGamePanel
    {
        private readonly VisualElement _root;

        public bool IsVisible => _root.style.display != DisplayStyle.None;

        public CreateGamePanel(VisualElement root)
        {
            _root = root;
            var rounds = root.Q<DropdownField>("NumOfRoundsDropdownField");
            if (rounds != null)
            {
                rounds.SetValueWithoutNotify(NetworkRequests.SelectedRoundLimit.ToString());
                rounds.RegisterValueChangedCallback(evt =>
                {
                    if (int.TryParse(evt.newValue, out var count) && (count == 8 || count == 10 || count == 12))
                        NetworkRequests.SelectedRoundLimit = count;
                });
            }
            Hide();
        }

        public void Show() => _root.style.display = DisplayStyle.Flex;
        public void Hide() => _root.style.display = DisplayStyle.None;
    }
}

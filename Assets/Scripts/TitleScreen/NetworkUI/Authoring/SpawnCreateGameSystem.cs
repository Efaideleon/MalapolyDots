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
            state.EntityManager.CreateSingleton(new CreateGameUIPanelComponent { Panel = createGamePanel });
        }

        public void OnStopRunning(ref SystemState state)
        {
            // nothing special on stop for now
        }

        public void OnDestroy(ref SystemState state)
        { }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct HideCreateGamePanelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CreateGameUIPanelComponent>();
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
            Hide();
        }

        public void Show() => _root.style.display = DisplayStyle.Flex;
        public void Hide() => _root.style.display = DisplayStyle.None;
    }
}

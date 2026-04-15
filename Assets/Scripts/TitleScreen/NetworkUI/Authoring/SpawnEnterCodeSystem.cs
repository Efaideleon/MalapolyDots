// csharp
using Assets.Common;
using Assets.Common.Assets.Common;
using TitleScreen.NetworkUI.Authoring;
using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SpawnEnterCodeSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnterCodeUIReference>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var uiRef = SystemAPI.ManagedAPI.GetSingleton<EnterCodeUIReference>();
            var prefab = uiRef.uiDocumentGO;
            if (prefab == null)
                return;

            Debug.Log("[SpawnEnterCodeUISystem] Spawning JoinByCode UI Document");
            var uiGameObject = Object.Instantiate(prefab);
            if (!uiGameObject.TryGetComponent<UIDocument>(out var uiDocument))
            {
                Object.Destroy(uiGameObject);
                return;
            }

            var root = uiDocument.rootVisualElement;
            var panel = new EnterCodePanel(root);

            // Create managed singleton component holding the panel instance.
            state.EntityManager.CreateSingleton(new EnterCodeUIPanelComponent { Panel = panel });
        }

        public void OnStopRunning(ref SystemState state)
        {
            // no-op
        }

        public void OnDestroy(ref SystemState state)
        {
            // no-op
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct HideEnterCodePanelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnterCodeUIPanelComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var menuPhase = SystemAPI.GetSingleton<GameMenuPhaseComponent>();
            var managed = SystemAPI.ManagedAPI.GetSingleton<EnterCodeUIPanelComponent>();
            var panel = managed?.Panel;
            if (panel == null)
                return;

            switch (menuPhase.Value)
            {
                case GameMenuPhase.JoinSetup:
                    if (!panel.IsVisible) panel.Show();
                    break;

                default:
                    if (panel.IsVisible) panel.Hide();
                    break;
            }
        }
    }

    // Managed component used as a singleton container for the JoinByCode panel.
    public class EnterCodeUIPanelComponent : IComponentData
    {
        public EnterCodePanel Panel;
    }
    // Simple wrapper around the UI Document root for the Join By Code UI.
    public class EnterCodePanel
    {
        private readonly VisualElement _root;
        public bool IsVisible => _root.style.display != DisplayStyle.None;

        public EnterCodePanel(VisualElement root)
        {
            _root = root;
            Hide();
        }

        public void Show() => _root.style.display = DisplayStyle.Flex;
        public void Hide() => _root.style.display = DisplayStyle.None;
    }
}

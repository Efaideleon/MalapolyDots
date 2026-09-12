using Assets.Scripts.DOTS.DataComponents;
using Assets.Common;
using Assets.Common.Assets.Common;
using TitleScreen.NetworkUI.Authoring;
using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using UnityEngine.UIElements;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SpawnJoinByCodeSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<JoinByCodeUIReference>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            Cleanup(ref state);
            var gameMenuRef = SystemAPI.ManagedAPI.GetSingleton<JoinByCodeUIReference>();
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

            var root = uiDocument.rootVisualElement;
            JoinSessionByCodePanel joinSessionByCodePanel = new(root);
            var panelEntity = state.EntityManager.CreateSingleton(new UIPanelComponent { JoinSessionByCodePanel = joinSessionByCodePanel });
            state.EntityManager.AddComponentObject(panelEntity, new GameObjectReference { Instance = uiGameObject });
        }

        public void OnStopRunning(ref SystemState state) => Cleanup(ref state);

        public void OnDestroy(ref SystemState state) => Cleanup(ref state);

        static void Cleanup(ref SystemState state)
        {
            state.EntityManager.DestroyEntity(state.GetEntityQuery(ComponentType.ReadOnly<UIPanelComponent>()));
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct HideJoinSessionByCodePanelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UIPanelComponent>();
            state.RequireForUpdate<GameMenuPhaseComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var menuPhase = SystemAPI.GetSingleton<GameMenuPhaseComponent>();
            //UnityEngine.Debug.Log($"[HideJoinSessionByCodePanelSystem] | GameMenuPhase {menuPhase.Value.ToString()}");
            var panel = SystemAPI.ManagedAPI.GetSingleton<UIPanelComponent>().JoinSessionByCodePanel;
            switch(menuPhase.Value)
            {
                case GameMenuPhase.JoinSession:
                    if(!panel.IsVisible)
                    {
                        //UnityEngine.Debug.Log($"[HideJoinSessionByCodePanelSystem] | panel visibility : {panel.IsVisible}");
                        if (SystemAPI.ManagedAPI.TryGetSingleton<GameMenuPanelsComponent>(out var menus))
                            foreach (var menu in menus.AllPanels) menu.Hide();
                        panel.Show();
                    }
                    break;
                default:
                    if(panel.IsVisible)
                    {
                        //UnityEngine.Debug.Log($"[HideJoinSessionByCodePanelSystem] | panel visibility : {panel.IsVisible}");
                        panel.Hide();
                    }
                    break;
            }
        }
    }

    public class UIPanelComponent : IComponentData
    {
        public JoinSessionByCodePanel JoinSessionByCodePanel;
    }

    public class JoinSessionByCodePanel
    {
        private readonly VisualElement _root;
        public bool IsVisible => _root.style.display != DisplayStyle.None;

        public JoinSessionByCodePanel(VisualElement root)
        {
            _root = root;
            Hide();
        }

        public void Hide() => _root.style.display = DisplayStyle.None;
        public void Show() => _root.style.display = DisplayStyle.Flex;
    }
}

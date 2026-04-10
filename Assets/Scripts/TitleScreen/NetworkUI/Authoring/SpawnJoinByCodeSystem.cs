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
            state.EntityManager.CreateSingleton(new UIPanelComponent { JoinSessionByCodePanel = joinSessionByCodePanel });
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        { }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct HideJoinSessionByCodePanelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UIPanelComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var menuPhase = SystemAPI.GetSingleton<GameMenuPhaseComponent>();
            UnityEngine.Debug.Log($"[HideJoinSessionByCodePanelSystem] | GameMenuPhase {menuPhase.Value.ToString()}");
            switch(menuPhase.Value)
            {
                case GameMenuPhase.CharacterSelect:
                    var panel = SystemAPI.ManagedAPI.GetSingleton<UIPanelComponent>().JoinSessionByCodePanel;
                    if(panel.IsVisible)
                    {
                        UnityEngine.Debug.Log($"[HideJoinSessionByCodePanelSystem] | panel visibility : {panel.IsVisible}");
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
        }

        public void Hide() => _root.style.display = DisplayStyle.None;
    }
}

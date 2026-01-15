using DOTS.DataComponents;
using TitleScreen.NetworkUI.Components;
using TitleScreen.NetworkUI.Panels;
using TitleScreen.NetworkUI.Systems;
using Unity.Entities;

namespace DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    // Run in the presentation system group.
    // There should be a system foreach panel to show or hide it.
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct LobbyUISystem : ISystem
    {
        private ComponentLookup<GameMenuPhaseComponent> menuPhaseLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameMenuPanelsComponent>();
            state.RequireForUpdate<GameMenuPhaseComponent>();
            state.RequireForUpdate<NetworkRoleTypeComponent>();

            menuPhaseLookup = SystemAPI.GetComponentLookup<GameMenuPhaseComponent>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            menuPhaseLookup.Update(ref state);
            var menuPhaseEntity = SystemAPI.GetSingletonEntity<GameMenuPhaseComponent>();

            // If there is no change in menu, return.
            if (!menuPhaseLookup.DidChange(menuPhaseEntity, state.LastSystemVersion))
                return;

            // if the menu is not showing lobby, return.
            if (menuPhaseLookup[menuPhaseEntity].Value != GameMenuPhase.Lobby)
                return;

            if (SystemAPI.ManagedAPI.TryGetSingleton<GameMenuPanelsComponent>(out var panelsComponent))
            {
                if (panelsComponent.PanelLookup != null && panelsComponent.AllPanels != null)
                {
                    foreach (var panel in panelsComponent.AllPanels)
                    {
                        panel.Hide();
                    }
                    panelsComponent.PanelLookup[GameMenuPhase.Lobby].Show();
                }

                var networkRole = SystemAPI.GetSingleton<NetworkRoleTypeComponent>().Value;
                UnityEngine.Debug.Log($"[LobbyUISystem] | networkRole: {networkRole.ToString()}");

                // If our network role is client, then disable the start button.  
                if (networkRole != NetworkRole.Client)
                    return;

                LobbyPanel lobbyPanel = panelsComponent.PanelLookup[GameMenuPhase.Lobby] as LobbyPanel;
                switch (networkRole)
                {
                    case NetworkRole.Client:
                        lobbyPanel.HideStartButton();
                        break;
                }
            }
        }
    }
}

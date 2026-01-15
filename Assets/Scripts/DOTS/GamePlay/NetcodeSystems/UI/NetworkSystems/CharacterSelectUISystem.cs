using TitleScreen.NetworkUI.Components;
using TitleScreen.NetworkUI.Systems;
using Unity.Entities;

namespace DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CharacterSelectUISystem : ISystem
    {
        private ComponentLookup<GameMenuPhaseComponent> menuPhaseLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameMenuPanelsComponent>();
            state.RequireForUpdate<GameMenuPhaseComponent>();
            menuPhaseLookup = SystemAPI.GetComponentLookup<GameMenuPhaseComponent>(true);
        }
        public void OnUpdate(ref SystemState state)
        {
            menuPhaseLookup.Update(ref state);
            var menuPhaseEntity = SystemAPI.GetSingletonEntity<GameMenuPhaseComponent>();

            if (!menuPhaseLookup.DidChange(menuPhaseEntity, state.LastSystemVersion))
                return;

            if (menuPhaseLookup[menuPhaseEntity].Value == GameMenuPhase.CharacterSelect)
            {
                var panelsComponent = SystemAPI.ManagedAPI.GetSingleton<GameMenuPanelsComponent>();
                if (panelsComponent.PanelLookup != null && panelsComponent.AllPanels != null)
                {
                    foreach (var panel in panelsComponent.AllPanels)
                    {
                        panel.Hide();
                    }
                    panelsComponent.PanelLookup[GameMenuPhase.CharacterSelect].Show();
                }
            }
        }
    }
}

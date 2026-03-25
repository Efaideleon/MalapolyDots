using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.Mediator.Systems;
using DOTS.DataComponents;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.StatsPanelSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct OnSelectStatsPanelSystem : ISystem
    {
        public BufferLookup<ChangeTurnEvent> changeTurnEventBufferLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NameComponent>();
            state.RequireForUpdate<MoneyComponent>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GhostDataLoadedTag>();
            state.RequireForUpdate<GameScreenInitializedFlag>();
            state.RequireForUpdate<StatsPanelRegistrationCompleteTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var activePlayer in SystemAPI.Query<RefRO<CurrentActivePlayer>>().WithChangeFilter<CurrentActivePlayer>())
            {
                var activePlayerName = SystemAPI.GetComponent<NameComponent>(activePlayer.ValueRO.Entity);

                UnityEngine.Debug.Log($"[OnSelectStatsPanelSystem] | Is This runnnig 1?");

                PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                panelControllers.statsPanelController.SetCurrentPanel(activePlayerName.Value);
            }

            if (shiftPanels)
            {
                var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
                var activePlayerName = SystemAPI.GetComponent<NameComponent>(activePlayerEntity);

                UnityEngine.Debug.Log($"[OnSelectStatsPanelSystem] | Is This runnnig 1?");

                PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelControllers?.statsPanelController != null)
                {
                    UnityEngine.Debug.Log($"[OnSelectStatsPanelSystem] | Is This runnnig 2 name: {activePlayerName.Value}");
                    panelControllers.statsPanelController.SelectPanel(activePlayerName.Value);
                    panelControllers.statsPanelController.ShiftPanelsRegistry();
                    panelControllers.statsPanelController.TranslateAllPanels();
                }
            }
        }
    }
}

using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.StatsPanelSystems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct OnSelectStatsPanelSystem : ISystem
    {
        public BufferLookup<ChangeTurnEvent> changeTurnEventBufferLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NameComponent>();
            state.RequireForUpdate<MoneyComponent>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameScreenInitializedFlag>();

            changeTurnEventBufferLookup = SystemAPI.GetBufferLookup<ChangeTurnEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var lastSystemVersion = state.LastSystemVersion;
            changeTurnEventBufferLookup.Update(ref state);

            var changeTurnEntity = SystemAPI.GetSingletonEntity<ChangeTurnEvent>();

            var shiftPanels = false;

            if (changeTurnEventBufferLookup.DidChange(changeTurnEntity, lastSystemVersion))
            {
                var buffer = changeTurnEventBufferLookup[changeTurnEntity];
                foreach (var e in buffer)
                {
                    shiftPanels = true;
                }
                buffer.Clear();
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

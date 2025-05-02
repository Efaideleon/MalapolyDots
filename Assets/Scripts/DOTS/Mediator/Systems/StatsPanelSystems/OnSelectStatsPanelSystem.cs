using DOTS.Characters;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.StatsPanelSystems
{
    public partial struct OnSelectStatsPanelSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerID>();
            state.RequireForUpdate<NameComponent>();
            state.RequireForUpdate<MoneyComponent>();
            state.RequireForUpdate<PanelControllers>();
            state.RequireForUpdate<CurrentPlayerID>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<ChangeTurnBufferEvent>>().WithChangeFilter<ChangeTurnBufferEvent>())
            {
                foreach (var e in buffer)
                {
                    foreach (var (playerID, name) in
                            SystemAPI.Query<
                            RefRO<PlayerID>,
                            RefRO<NameComponent>
                            >())
                    {
                        PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                        if (panelControllers != null)
                        {
                            if (panelControllers.statsPanelController != null)
                            {
                                var currPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();
                                if (playerID.ValueRO.Value == currPlayerID.Value)
                                {
                                    var playerName = name.ValueRO.Value;
                                    panelControllers.statsPanelController.SelectPanel(playerName);
                                    panelControllers.statsPanelController.TranslateAllPanels();
                                    panelControllers.statsPanelController.ShiftPanelsRegistry();

                                }
                            }
                        }
                    }
                }
                buffer.Clear();
            }
        }
    }
}

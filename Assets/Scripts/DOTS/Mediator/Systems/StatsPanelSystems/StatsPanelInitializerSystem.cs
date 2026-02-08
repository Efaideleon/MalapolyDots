using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.Mediator.Systems;
using DOTS.DataComponents;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.StatsPanelSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct StatsPanelInitializerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MoneyComponent>();
            state.RequireForUpdate<PanelControllers>();
            state.RequireForUpdate<CurrentPlayerID>();
            state.RequireForUpdate<GameScreenInitializedFlag>();
            state.RequireForUpdate<StatsPanelRegistrationCompleteTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // This system should only run when all the panel geometry has been calculated.
            // But the first the panel needs to placed on screen.
            // Currently No panels are screen when this system runs.
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();

            if (!panelControllers.statsPanelController.AllPanelsOnScreen)
            {
                UnityEngine.Debug.Log($"[InitializeStatsPanelSystem] | Not all stats panels on screen.");
                return;
            }

            foreach (var (name, money) in SystemAPI.Query<RefRO<NameComponent>, RefRO<MoneyComponent>>())
            {
                if (panelControllers != null)
                {
                    if (panelControllers.statsPanelController != null)
                    {
                        StatsPanelContext newContext = new()
                        {
                            Name = name.ValueRO.Value,
                            Money = money.ValueRO.Value.ToString()
                        };
                        panelControllers.statsPanelController.Context = newContext;
                        panelControllers.statsPanelController.InitializePanel();
                    }
                }
            }

            if (panelControllers?.statsPanelController != null)
            {
                if (panelControllers?.statsPanelController.SmallPanelsContainer.resolvedStyle.width > 0)
                {
                    UnityEngine.Debug.Log($"[StatsPanelInitializerSystem] | StatsPanelRegistry size: {panelControllers.statsPanelController.StatsPanelRegistry.Count}");
                    panelControllers.statsPanelController.HighlightPanel(0);

                    // This method should be only after all the stats panels appear on screen.
                    // It sets the first panels as the current player's panel.
                    panelControllers.statsPanelController.SetPanelsInitialPositions();
                    //panelControllers.statsPanelController.ShiftPanelsRegistry();
                    state.Enabled = false;
                }
            }
        }
    }
}

using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.StatsPanelSystems
{
    public partial struct StatsPanelInitializerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MoneyComponent>();
            state.RequireForUpdate<PanelControllers>();
            state.RequireForUpdate<CurrentPlayerID>();
            state.RequireForUpdate<GameScreenInitializedFlag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // This system should only run when all the panel geometry has been calculated.
            // But the first the panel needs to placed on screen.
            // Currently No panels are screen when this system runs.
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            foreach (var (name, money) in
                    SystemAPI.Query<RefRO<NameComponent>, RefRO<MoneyComponent>>().WithChangeFilter<MoneyComponent>())
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
                    panelControllers.statsPanelController.SelectPanel(0);
                    panelControllers.statsPanelController.SetPanelsInitialPositions();
                    //panelControllers.statsPanelController.ShiftPanelsRegistry();
                    state.Enabled = false;
                }
            }
        }
    }
}

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
        }

        public void OnUpdate(ref SystemState state)
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            bool allPanelsInitialized = false;
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
                        allPanelsInitialized = true;
                    }
                }
            }

            if (allPanelsInitialized)
                if (panelControllers != null)
                    if (panelControllers.statsPanelController != null)
                    {
                        panelControllers.statsPanelController.SelectPanel(0);
                        panelControllers.statsPanelController.SetPanelsInitialPositions();
                        panelControllers.statsPanelController.ShiftPanelsRegistry();
                        state.Enabled = false;
                    }
        }
    }
}

using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.RollPanelSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct RollPanelUpdaterManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RollAmountCountDown>();
            state.RequireForUpdate<PanelControllers>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var rollAmount in SystemAPI.Query<RefRO<RollAmountCountDown>>().WithChangeFilter<RollAmountCountDown>())
            {
                PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelControllers != null)
                {
                    if (panelControllers.rollPanelController != null)
                    {
                        panelControllers.rollPanelController.Update(rollAmount.ValueRO.Value);
                    }
                }
            }
        }
    }
}

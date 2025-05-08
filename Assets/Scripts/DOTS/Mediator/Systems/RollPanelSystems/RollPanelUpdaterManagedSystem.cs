using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;

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
                    RollPanelContext rollPanelContext = new(){ AmountRolled = rollAmount.ValueRO.Value };
                    panelControllers.rollPanelController.Context = rollPanelContext;
                    panelControllers.rollPanelController.Update();
                }
            }
        }
    }
}

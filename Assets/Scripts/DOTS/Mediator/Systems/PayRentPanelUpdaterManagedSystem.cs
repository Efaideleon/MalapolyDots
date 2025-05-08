using DOTS.UI.Controllers;
using DOTS.UI.Systems;
using Unity.Entities;

public partial struct PayRentPanelUpdaterManagedSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PayRentPanelContextComponent>();
        state.RequireForUpdate<PanelControllers>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var payRentPanelContext in SystemAPI.Query<
                RefRO<PayRentPanelContextComponent>
                >()
                .WithChangeFilter<PayRentPanelContextComponent>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                if (panelControllers.payRentPanelController != null)
                {
                    panelControllers.payRentPanelController.Context = payRentPanelContext.ValueRO.Value;
                    panelControllers.payRentPanelController.Update();
                }
            }
        }
    }
}

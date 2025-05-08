using DOTS.UI.Controllers;
using DOTS.UI.Systems;
using Unity.Entities;

public partial struct PurchaseHousePanelUpdaterManagedSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PurhcaseHousePanelContextComponent>();
        state.RequireForUpdate<PanelControllers>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var purchaseHousePanelContext in
                SystemAPI.Query<
                RefRO<PurhcaseHousePanelContextComponent>
                >()
                .WithChangeFilter<PurhcaseHousePanelContextComponent>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                if (panelControllers.purchaseHousePanelController != null)
                {
                    panelControllers.purchaseHousePanelController.PurchaseHousePanel.Context = purchaseHousePanelContext.ValueRO.Value;
                    panelControllers.purchaseHousePanelController.PurchaseHousePanel.Update();
                }
            }
        }

    }
}

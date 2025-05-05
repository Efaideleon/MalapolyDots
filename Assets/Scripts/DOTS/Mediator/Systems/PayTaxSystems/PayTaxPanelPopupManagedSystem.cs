using DOTS.UI.Controllers;
using Unity.Entities;

public partial struct PayTaxPanelPopupManagedSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ShowPayTaxPanelBuffer>();
        state.RequireForUpdate<PanelControllers>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var buffer in SystemAPI.Query<DynamicBuffer<ShowPayTaxPanelBuffer>>().WithChangeFilter<ShowPayTaxPanelBuffer>())
        {
            foreach (var e in buffer)
            {
                var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelControllers == null)
                    break;
                if (panelControllers.payTaxPanelController == null)
                    break;
                if (panelControllers.backdropController == null)
                    break;

                panelControllers.payTaxPanelController.ShowPanel();
                panelControllers.backdropController.ShowBackdrop();
            }
            buffer.Clear();
        }
    }
}

using DOTS.UI.Controllers;
using Unity.Entities;

public partial struct SpaceActionsPanelPopupManagedSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ShowActionsPanelBuffer>();
        state.RequireForUpdate<PanelControllers>();
    }
    public void OnUpdate(ref SystemState state)
    {
        foreach (var buffer in SystemAPI.Query<DynamicBuffer<ShowActionsPanelBuffer>>().WithChangeFilter<ShowActionsPanelBuffer>())
        {
            foreach (var e in buffer)
            {
                var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelControllers == null)
                    break;
                if (panelControllers.spaceActionsPanelController == null)
                    break;
                if (panelControllers.backdropController == null)
                    break;

                panelControllers.spaceActionsPanelController.SpaceActionsPanel.Show();
                panelControllers.backdropController.ShowBackdropWithDelay();
            }
            buffer.Clear();
        }
    }
}

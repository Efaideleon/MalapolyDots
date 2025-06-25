using DOTS.EventBuses;
using DOTS.UI.Controllers;
using Unity.Entities;

public partial struct BackDropSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<BackDropEventBus>();
        state.RequireForUpdate<PanelControllers>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var buffer in SystemAPI.Query<DynamicBuffer<BackDropEventBus>>().WithChangeFilter<BackDropEventBus>())
        {
            var controller = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            // this is bogus as it the buffer won't get cleared if the controller is null
            if (controller == null)
                break;
            if (controller.backdropController == null)
                break;

            foreach (var _ in buffer)
            {
                UnityEngine.Debug.Log("[BackDropSystem] | Hiding Panels from the Backdrop system");
                controller.backdropController.HidePanelsAndButton();
            }

            buffer.Clear();
        }
    }
}

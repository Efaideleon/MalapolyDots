using DOTS.Mediator.Systems.ChancePanelSystems;
using DOTS.UI.Controllers;
using Unity.Entities;

public partial struct ChancePanelPopupManagedSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ShowChancePanelBuffer>();
        state.RequireForUpdate<PanelControllers>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var buffer in SystemAPI.Query<DynamicBuffer<ShowChancePanelBuffer>>().WithChangeFilter<ShowChancePanelBuffer>())
        {
            foreach (var e in buffer)
            {
                var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelControllers == null)
                    break;
                if (panelControllers.chancePanelController == null)
                    break;
                if (panelControllers.backdropController == null)
                    break;

                UnityEngine.Debug.Log("Shwoing Chance Panel!");
                panelControllers.chancePanelController.ShowPanel();
                panelControllers.backdropController.ShowBackdrop();
            }
            buffer.Clear();
        }

    }
}

using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.GoPanelSystems
{
    public partial struct GoPanelPopupManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ShowGoPanelBuffer>();
            state.RequireForUpdate<PanelControllers>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<ShowGoPanelBuffer>>().WithChangeFilter<ShowGoPanelBuffer>())
            {
                foreach (var e in buffer)
                {
                    var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                    if (panelControllers == null)
                        break;
                    if (panelControllers.goPanelController == null)
                        break;
                    if (panelControllers.backdropController == null)
                        break;

                    UnityEngine.Debug.Log("[GoPanelPopupManagedSystem] | Showing Go Panel!");
                    panelControllers.goPanelController.ShowPanel();
                    panelControllers.backdropController.ShowBackdrop();
                }
                buffer.Clear();
            }

        }
    }
}

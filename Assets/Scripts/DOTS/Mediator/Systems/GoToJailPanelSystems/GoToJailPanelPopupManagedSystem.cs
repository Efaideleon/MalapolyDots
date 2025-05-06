using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems
{
    public partial struct GoToJailPanelPopupManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ShowGoToJailPanelBuffer>();
            state.RequireForUpdate<PanelControllers>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<ShowGoToJailPanelBuffer>>().WithChangeFilter<ShowGoToJailPanelBuffer>())
            {
                foreach (var e in buffer)
                {
                    var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                    if (panelControllers == null)
                        break;
                    if (panelControllers.goToJailPanelController == null)
                        break;
                    if (panelControllers.backdropController == null)
                        break;

                    panelControllers.goToJailPanelController.ShowPanel();
                }
                buffer.Clear();
            }
        }
    }
}

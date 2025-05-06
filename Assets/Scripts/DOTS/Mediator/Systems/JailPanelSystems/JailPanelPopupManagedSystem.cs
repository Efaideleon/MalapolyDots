using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.JailPanelSystems
{
    public partial struct JailPanelPopupManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ShowJailPanelBuffer>();
            state.RequireForUpdate<PanelControllers>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<ShowJailPanelBuffer>>().WithChangeFilter<ShowJailPanelBuffer>())
            {
                foreach (var e in buffer)
                {
                    var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                    if (panelControllers == null)
                        break;
                    if (panelControllers.jailPanelController == null)
                        break;
                    if (panelControllers.backdropController == null)
                        break;

                    panelControllers.jailPanelController.ShowPanel();
                }
                buffer.Clear();
            }
        }
    }
}

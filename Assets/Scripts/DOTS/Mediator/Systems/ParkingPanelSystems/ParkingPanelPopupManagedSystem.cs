using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.UI.Mediator.Systems.ParkingPanelSystems
{
    public partial struct ParkingPanelPopupManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ShowParkingPanelBuffer>();
            state.RequireForUpdate<PanelControllers>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<ShowParkingPanelBuffer>>().WithChangeFilter<ShowParkingPanelBuffer>())
            {
                foreach (var e in buffer)
                {
                    var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                    if (panelControllers == null)
                        break;
                    if (panelControllers.parkingPanelController == null)
                        break;
                    if (panelControllers.backdropController == null)
                        break;

                    panelControllers.parkingPanelController.ShowPanel();
                }
                buffer.Clear();
            }

        }
    }
}

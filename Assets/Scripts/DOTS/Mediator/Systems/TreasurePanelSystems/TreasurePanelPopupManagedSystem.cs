using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.TreasurePanelSystems
{
    public partial struct TreasurePanelPopupManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ShowTreasurePanelBuffer>();
            state.RequireForUpdate<PanelControllers>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var buffer in SystemAPI.Query<DynamicBuffer<ShowTreasurePanelBuffer>>().WithChangeFilter<ShowTreasurePanelBuffer>())
            {
                foreach (var e in buffer)
                {
                    var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                    if (panelControllers == null)
                        break;
                    if (panelControllers.treasurePanelController == null)
                        break;
                    if (panelControllers.backdropController == null)
                        break;

                    panelControllers.treasurePanelController.ShowPanel();
                }
                buffer.Clear();
            }
        }
    }
}

using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.TreasurePanelSystems
{
    public partial struct TreasurePanelContextUpdaterManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TreasureCard>();
            state.RequireForUpdate<PanelControllers>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var treasureCard in SystemAPI.Query<RefRO<TreasureCard>>().WithChangeFilter<TreasureCard>())
            {
                var panelcontrollers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelcontrollers == null)
                   break;
                if (panelcontrollers.treasurePanelController == null)
                    break;

                panelcontrollers.treasurePanelController.Context = new TreasurePanelContext 
                { 
                    Title = treasureCard.ValueRO.data.ToString()
                };
                panelcontrollers.treasurePanelController.Update();
            }
        }
    }
}

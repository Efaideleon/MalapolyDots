using DOTS.DataComponents;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.ChancePanelSystems
{
    public partial struct ChancePanelPopupUpdaterManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ChanceCardPicked>();
            state.RequireForUpdate<PanelControllers>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var cardPicked in SystemAPI.Query<RefRO<ChanceCardPicked>>().WithChangeFilter<ChanceCardPicked>())
            {
                PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelControllers != null)
                {
                    if (panelControllers.chancePanelController != null)
                    {
                        var context = new ChancePanelContext { Title = cardPicked.ValueRO.msg.ToString() };
                        panelControllers.chancePanelController.Update(ref context);
                    }
                }
            }
        }
    }
}

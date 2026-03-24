using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.UI.Controllers;
using DOTS.UI.Controllers;
using Unity.Entities;

namespace DOTS.Mediator.Systems.ChancePanelSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ChancePanelPopupUpdaterManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GhostChanceCardPicked>();
            state.RequireForUpdate<PanelControllerService>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // TODO: When we landed on a chance, the server picked a chance card for that client.
            // Now we need show the conent of the card to the client.
            foreach (var cardPicked in SystemAPI.Query<RefRO<GhostChanceCardPicked>>().WithChangeFilter<GhostChanceCardPicked>().WithAll<ActivePlayer>())
            {
                var service = SystemAPI.ManagedAPI.GetSingleton<PanelControllerService>();
                if (service.TryGet<ChancePanelController>(out var chancePanel))
                {
                    var context = new ChancePanelContext { Title = cardPicked.ValueRO.msg.ToString() };
                    chancePanel.Update(context);
                }
            }
        }
    }
}

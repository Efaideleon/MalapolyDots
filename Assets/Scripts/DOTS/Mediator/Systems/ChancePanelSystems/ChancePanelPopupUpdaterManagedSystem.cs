using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.UI.Controllers;
using DOTS.UI.Controllers;
using DOTS.DataComponents;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.Mediator.Systems.ChancePanelSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ChancePanelPopupUpdaterManagedSystem : ISystem
    {
        private uint lastPresentedDraw;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GhostChanceCardPicked>();
            state.RequireForUpdate<PanelControllerService>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // TODO: When we landed on a chance, the server picked a chance card for that client.
            // Now we need show the conent of the card to the client.
            foreach (var cardPicked in SystemAPI.Query<RefRO<GhostChanceCardPicked>>().WithAll<GhostOwnerIsLocal>())
            {
                if (cardPicked.ValueRO.DrawSequence == 0 || cardPicked.ValueRO.DrawSequence == lastPresentedDraw) continue;
                var service = SystemAPI.ManagedAPI.GetSingleton<PanelControllerService>();
                if (service.TryGet<ChancePanelController>(out var chancePanel))
                {
                    var card = cardPicked.ValueRO;
                    string message = card.msg.ToString();
                    if (card.amount != 0)
                        message += card.amount > 0 ? $"\nCollect Q{card.amount:N0}" : $"\nPay Q{-(long)card.amount:N0}";
                    else if (card.effect == ChanceEffect.BuildingRepairs)
                        message += "\nNo repair costs this time!";
                    else if (card.effect == ChanceEffect.PropertyIncome)
                        message += "\nNo properties owned yet.";
                    else if (card.effect == ChanceEffect.PayEachPlayer || card.effect == ChanceEffect.CollectFromEachPlayer)
                        message += "\nNo other players to pay.";
                    var context = new ChancePanelContext { Title = message };
                    chancePanel.Update(context);
                    lastPresentedDraw = cardPicked.ValueRO.DrawSequence;
                    chancePanel.Show();
                }
            }
        }
    }
}

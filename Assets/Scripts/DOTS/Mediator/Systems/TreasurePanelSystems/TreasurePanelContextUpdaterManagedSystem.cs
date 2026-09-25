using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.UI.Controllers;
using DOTS.UI.Controllers;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.Mediator.Systems.TreasurePanelSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct TreasurePanelContextUpdaterManagedSystem : ISystem
    {
        private uint lastPresentedDraw;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GhostTreasureCardPicked>();
            state.RequireForUpdate<PanelControllerService>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var treasureCard in SystemAPI.Query<RefRO<GhostTreasureCardPicked>>().WithAll<GhostOwnerIsLocal>())
            {
                if (treasureCard.ValueRO.DrawSequence == 0 || treasureCard.ValueRO.DrawSequence == lastPresentedDraw) continue;
                var service = SystemAPI.ManagedAPI.GetSingleton<PanelControllerService>();
                if (service.TryGet<TreasurePanelController>(out var treasurePanel))
                {
                    UnityEngine.Debug.Log($"treasureCard msg: {treasureCard.ValueRO.msg}");
                    var context = new TreasurePanelContext { Title = treasureCard.ValueRO.msg.ToString(), Amount = treasureCard.ValueRO.amount.ToString() };
                    treasurePanel.Update(context);
                    lastPresentedDraw = treasureCard.ValueRO.DrawSequence;
                    treasurePanel.Show();
                }
            }
        }
    }
}

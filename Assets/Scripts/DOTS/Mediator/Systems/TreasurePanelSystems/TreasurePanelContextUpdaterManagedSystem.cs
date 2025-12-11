using DOTS.GamePlay;
using Unity.Entities;

namespace DOTS.Mediator.Systems.TreasurePanelSystems
{
    public partial struct TreasurePanelContextUpdaterManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TreasureCard>();
            state.RequireForUpdate<TreasurePanelData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var treasureCard in SystemAPI.Query<RefRO<TreasureCard>>().WithChangeFilter<TreasureCard>())
            {
                SystemAPI.ManagedAPI.GetSingleton<TreasurePanelData>().Panel.TitleLabel = treasureCard.ValueRO.data.ToString();
            }
        }
    }
}

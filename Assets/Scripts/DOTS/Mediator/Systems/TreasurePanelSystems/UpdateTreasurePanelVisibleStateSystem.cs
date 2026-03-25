using DOTS.Mediator.Authoring;
using Unity.Entities;

namespace DOTS.Mediator.Systems
{
    public partial struct UpdateTreasurePanelVisibleStateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TreasurePanelTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            
        }
    }
}

using DOTS.EventBuses;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public partial struct SamePropertyClickSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // state.EntityManager.CreateSingletonBuffer<BackDropEventBus>();
        }

        public void OnUpdate(ref SystemState state)
        { }
    }
}

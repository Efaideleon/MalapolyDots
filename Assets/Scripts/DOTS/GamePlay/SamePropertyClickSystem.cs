using DOTS.EventBuses;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public partial struct SamePropertyClickSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IsSamePropertyClicked>();
            state.RequireForUpdate<BackDropEventBus>();
            state.EntityManager.CreateSingletonBuffer<BackDropEventBus>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var flag in SystemAPI.Query<RefRO<IsSamePropertyClicked>>().WithChangeFilter<IsSamePropertyClicked>())
            {
                UnityEngine.Debug.Log($"IsSamePropertyClicked changed {flag.ValueRO.Value}");
                var buffer = SystemAPI.GetSingletonBuffer<BackDropEventBus>();
                buffer.Add(new BackDropEventBus { });
            }
        }
    }
}

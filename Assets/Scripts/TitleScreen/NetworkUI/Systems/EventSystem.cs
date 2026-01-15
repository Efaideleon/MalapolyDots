using TitleScreen.NetworkUI.Components;
using Unity.Entities;

namespace TitleScreen.NetworkUI.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct EventSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameMenuPanelsComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.ManagedAPI.TryGetSingleton<GameMenuPanelsComponent>(out var _))
                return;

            var ecb = GetECB(ref state);
            foreach (var (_, entity) in SystemAPI.Query<RefRO<UIEvent>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
        }

        private readonly EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        }
    }
}

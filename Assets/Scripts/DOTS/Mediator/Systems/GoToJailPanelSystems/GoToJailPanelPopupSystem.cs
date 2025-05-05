using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Mediator.Systems
{
    public struct ShowGoToJailPanelBuffer : IBufferElementData
    { }

    [BurstCompile]
    public partial struct GoToJailPanelPopupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<ShowGoToJailPanelBuffer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<LandedOnSpace>();
            state.RequireForUpdate<GoToJailTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    var landedOnEntity = SystemAPI.GetSingleton<LandedOnSpace>();
                    if (SystemAPI.HasComponent<GoToJailTag>(landedOnEntity.entity))
                        SystemAPI.GetSingletonBuffer<ShowGoToJailPanelBuffer>().Add(new ShowGoToJailPanelBuffer { });
                }

        }
    }
}

using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Mediator.Systems.ChancePanelSystems
{
    public struct ShowChancePanelBuffer : IBufferElementData
    { }

    [BurstCompile]
    public partial struct ChancePanelPopupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<ShowChancePanelBuffer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<LandedOnSpace>();
            state.RequireForUpdate<ChanceSpaceTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    var landedOnEntity = SystemAPI.GetSingleton<LandedOnSpace>();
                    if (SystemAPI.HasComponent<ChanceSpaceTag>(landedOnEntity.entity))
                        SystemAPI.GetSingletonBuffer<ShowChancePanelBuffer>().Add(new ShowChancePanelBuffer { });
                }
        }
    }
}

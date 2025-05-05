using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Mediator.Systems.TreasurePanelSystems
{
    public struct ShowTreasurePanelBuffer : IBufferElementData
    { }

    [BurstCompile]
    public partial struct TreasurePanelPopupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<ShowTreasurePanelBuffer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<LandedOnSpace>();
            state.RequireForUpdate<TreasureSpaceTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    var landedOnEntity = SystemAPI.GetSingleton<LandedOnSpace>();
                    if (SystemAPI.HasComponent<TreasureSpaceTag>(landedOnEntity.entity))
                        SystemAPI.GetSingletonBuffer<ShowTreasurePanelBuffer>().Add(new ShowTreasurePanelBuffer { });
                }
        }
    }
}

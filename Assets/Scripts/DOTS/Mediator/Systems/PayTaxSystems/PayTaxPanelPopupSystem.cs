using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Mediator.Systems.PayTaxSystems
{
    public struct ShowPayTaxPanelBuffer : IBufferElementData
    { }

    [BurstCompile]
    public partial struct PayTaxPanelPopupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<ShowPayTaxPanelBuffer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<LandedOnSpace>();
            state.RequireForUpdate<TaxSpaceTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    var landedOnEntity = SystemAPI.GetSingleton<LandedOnSpace>();
                    if (SystemAPI.HasComponent<TaxSpaceTag>(landedOnEntity.entity))
                        SystemAPI.GetSingletonBuffer<ShowPayTaxPanelBuffer>().Add(new ShowPayTaxPanelBuffer { });
                }
        }
    }
}

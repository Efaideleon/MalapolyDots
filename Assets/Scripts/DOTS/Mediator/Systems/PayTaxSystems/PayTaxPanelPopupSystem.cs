using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Entities;

public struct ShowPayTaxPanelBuffer : IBufferElementData
{ }

public partial struct PayTaxPanelPopupSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingletonBuffer<ShowPayTaxPanelBuffer>();
        state.RequireForUpdate<GameStateComponent>();
        state.RequireForUpdate<LandedOnSpace>();
        state.RequireForUpdate<TaxSpaceTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            if (gameState.ValueRO.State == GameState.Landing)
            {
                var landedOnEntity = SystemAPI.GetSingleton<LandedOnSpace>();
                // Change PropertySpaceTag to TaxSpaceTag
                if (SystemAPI.HasComponent<TaxSpaceTag>(landedOnEntity.entity))
                    SystemAPI.GetSingletonBuffer<ShowPayTaxPanelBuffer>().Add(new ShowPayTaxPanelBuffer { });
            }
    }
}

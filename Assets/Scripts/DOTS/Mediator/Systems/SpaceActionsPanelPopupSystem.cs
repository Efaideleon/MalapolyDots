using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Entities;

public struct ShowActionsPanelBuffer : IBufferElementData
{ }

public partial struct SpaceActionsPanelPopupSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingletonBuffer<ShowActionsPanelBuffer>();
        state.RequireForUpdate<GameStateComponent>();
        state.RequireForUpdate<LandedOnSpace>();
        state.RequireForUpdate<PropertySpaceTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            if (gameState.ValueRO.State == GameState.Landing)
            {
                var landedOnEntity = SystemAPI.GetSingleton<LandedOnSpace>();
                if (SystemAPI.HasComponent<PropertySpaceTag>(landedOnEntity.entity))
                    SystemAPI.GetSingletonBuffer<ShowActionsPanelBuffer>().Add(new ShowActionsPanelBuffer { });
            }
    }
}

using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

public struct ShowActionsPanelBuffer : IBufferElementData
{ }

[BurstCompile]
public partial struct SpaceActionsPanelPopupSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingletonBuffer<ShowActionsPanelBuffer>();
        state.RequireForUpdate<GameStateComponent>();
        state.RequireForUpdate<LandedOnSpace>();
        state.RequireForUpdate<PropertySpaceTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            if (gameState.ValueRO.State == GameState.Landing)
            {
                UnityEngine.Debug.Log($"Sending event to show actions panels");
                var landedOnEntity = SystemAPI.GetSingleton<LandedOnSpace>();
                if (SystemAPI.HasComponent<PropertySpaceTag>(landedOnEntity.entity))
                    SystemAPI.GetSingletonBuffer<ShowActionsPanelBuffer>().Add(new ShowActionsPanelBuffer { });
            }
    }
}

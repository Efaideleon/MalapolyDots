using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Entities;

namespace DOTS.Mediator.Systems.JailPanelSystems
{
    public struct ShowJailPanelBuffer : IBufferElementData
    { }

    public partial struct JailPanelPopupSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<ShowJailPanelBuffer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<LandedOnSpace>();
            state.RequireForUpdate<JailSpaceTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    var landedOnEntity = SystemAPI.GetSingleton<LandedOnSpace>();
                    if (SystemAPI.HasComponent<JailSpaceTag>(landedOnEntity.entity))
                    {
                        SystemAPI.GetSingletonBuffer<ShowJailPanelBuffer>().Add(new ShowJailPanelBuffer { });
                    }
                }

        }
    }
}

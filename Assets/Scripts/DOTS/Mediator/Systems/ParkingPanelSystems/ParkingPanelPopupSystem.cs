using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.UI.Mediator.Systems.ParkingPanelSystems
{
    public struct ShowParkingPanelBuffer : IBufferElementData
    { }

    [BurstCompile]
    public partial struct ParkingPanelPopupSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<ShowParkingPanelBuffer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<LandedOnSpace>();
            state.RequireForUpdate<ParkingSpaceTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
                if (gameState.ValueRO.State == GameState.Landing)
                {
                    var landedOnEntity = SystemAPI.GetSingleton<LandedOnSpace>();
                    if (SystemAPI.HasComponent<ParkingSpaceTag>(landedOnEntity.entity))
                        SystemAPI.GetSingletonBuffer<ShowParkingPanelBuffer>().Add(new ShowParkingPanelBuffer { });
                }
        }
    }
}

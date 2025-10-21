using Unity.Entities;

namespace DOTS.GamePlay.CameraSystems
{
    public partial struct CameraWalkingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {}

        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                if (gameState.ValueRO.State == GameState.Walking)
                {
                    UnityEngine.Debug.Log("[CameraWalkingSystem] | Walking");
                }
            }
        }
    }
}

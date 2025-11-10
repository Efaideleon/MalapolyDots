using DOTS.Characters;
using DOTS.DataComponents;
using Unity.Entities;

namespace DOTS.GamePlay.CameraSystems
{
    public partial struct CameraPlayerSwitch : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<CurrentPlayerID>();
            state.RequireForUpdate<PivotTransform>();
            state.RequireForUpdate<CurrentPivotRotation>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerID in SystemAPI.Query<RefRO<CurrentPlayerID>>().WithChangeFilter<CurrentPlayerID>())
            {
                var pivot = SystemAPI.GetSingletonRW<PivotTransform>();
                var currentPlayer = SystemAPI.GetSingleton<CurrentPlayerComponent>();
                var currentPivotRotation = SystemAPI.GetComponent<CurrentPivotRotation>(currentPlayer.entity);
                pivot.ValueRW.Rotation = currentPivotRotation.Value;
                UnityEngine.Debug.Log($"[CameraPlayerSwitch] | rotation: {pivot.ValueRO.Rotation}");
                UnityEngine.Debug.Log($"[CameraPlayerSwitch] | player id: {playerID.ValueRO.Value}");
            }
        }
    }
}

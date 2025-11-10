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
            state.RequireForUpdate<PivotTransformTag>();
            state.RequireForUpdate<CurrentPivotRotation>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerID in SystemAPI.Query<RefRO<CurrentPlayerID>>().WithChangeFilter<CurrentPlayerID>())
            {
                var pivotRotation = SystemAPI.GetSingletonRW<PivotRotation>();
                var currentPlayer = SystemAPI.GetSingleton<CurrentPlayerComponent>();
                var currentPivotRotation = SystemAPI.GetComponent<CurrentPivotRotation>(currentPlayer.entity);
                pivotRotation.ValueRW.Value = currentPivotRotation.Value;
                UnityEngine.Debug.Log($"[CameraPlayerSwitch] | rotation: {pivotRotation.ValueRO.Value}");
                UnityEngine.Debug.Log($"[CameraPlayerSwitch] | player id: {playerID.ValueRO.Value}");
            }
        }
    }
}

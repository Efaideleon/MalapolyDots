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
            state.RequireForUpdate<PlayerToCameraAngleData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerID in SystemAPI.Query<RefRO<CurrentPlayerID>>().WithChangeFilter<CurrentPlayerID>())
            {
                var pivot = SystemAPI.GetSingletonRW<PivotTransform>();
                var playerToCameraRotation = SystemAPI.GetSingleton<PlayerToCameraAngleData>();
                playerToCameraRotation.Map.TryGetValue(playerID.ValueRO.Value, out var pivotRotation);
                pivot.ValueRW.Rotation = pivotRotation;
                UnityEngine.Debug.Log($"[CameraPlayerSwitch] | rotation: {pivot.ValueRO.Rotation}");
                UnityEngine.Debug.Log($"[CameraPlayerSwitch] | player id: {playerID.ValueRO.Value}");
            }
        }
    }
}

using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace DOTS.GamePlay.CameraSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct CameraPlayerSwitch : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<CurrentPlayerID>();
            state.RequireForUpdate<PivotTransformTag>();
            state.RequireForUpdate<CurrentPivotRotation>();
            state.RequireForUpdate<GhostDataLoadedTag>();
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerID in SystemAPI.Query<RefRO<CurrentPlayerID>>().WithChangeFilter<CurrentPlayerID>())
            {
                var pivotRotation = SystemAPI.GetSingletonRW<PivotRotation>();
                var currentPlayer = SystemAPI.GetSingleton<CurrentActivePlayer>();
                var currentPivotRotation = SystemAPI.GetComponent<CurrentPivotRotation>(currentPlayer.Entity);
                pivotRotation.ValueRW.Value = currentPivotRotation.Value;
                UnityEngine.Debug.Log($"[CameraPlayerSwitch] | pivotRotation {pivotRotation.ValueRO.Value}");
                UnityEngine.Debug.Log($"[CameraPlayerSwitch] | currentPivotRotation {currentPivotRotation.Value}");

                var playerPosition = SystemAPI.GetComponent<LocalTransform>(currentPlayer.Entity);
                UnityEngine.Debug.Log($"[CameraPlayerSwitch] | player position: {playerPosition.Position}");
            }
        }
    }
}

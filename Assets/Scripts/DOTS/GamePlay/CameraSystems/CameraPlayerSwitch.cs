using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters;
using DOTS.DataComponents;
using Unity.Entities;

namespace DOTS.GamePlay.CameraSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
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
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerID in SystemAPI.Query<RefRO<CurrentPlayerID>>().WithChangeFilter<CurrentPlayerID>())
            {
                var pivotRotation = SystemAPI.GetSingletonRW<PivotRotation>();
                var currentPlayer = SystemAPI.GetSingleton<CurrentActivePlayer>();
                var currentPivotRotation = SystemAPI.GetComponent<CurrentPivotRotation>(currentPlayer.Entity);
                pivotRotation.ValueRW.Value = currentPivotRotation.Value;
            }
        }
    }
}

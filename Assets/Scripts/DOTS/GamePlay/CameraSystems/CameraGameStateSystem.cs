using Assets.Scripts.DOTS.GamePlay;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay.CameraSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateAfter(typeof(PlayerEntityToGameObjectSystem))]
    public partial struct CameraGameStateSystem : ISystem
    {
        GameState lastState;
        bool hasState;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>())
            {
                if (CinemachineCameraManager.Instance == null || CameraTargetHolder.Instance == null || TargetPlace.Instance == null) continue;
                if (hasState && lastState == gameState.ValueRO.State) continue;
                lastState = gameState.ValueRO.State;
                hasState = true;
                switch (gameState.ValueRO.State)
                {
                    case GameState.Rolling:
                    case GameState.Walking:
                        CinemachineCameraManager.Instance.SwitchToFollowCamera();
                        break;

                    case GameState.Landing:
                        CinemachineCameraManager.Instance.SwitchToLandingCamera(CameraTargetHolder.Instance.transform, TargetPlace.Instance.transform);
                        break;
                }
            }
        }
    }
}

using Assets.Scripts.DOTS.GamePlay;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay.CameraSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct CameraGameStateSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
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

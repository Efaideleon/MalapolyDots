using PerspectiveCam = DOTS.GamePlay.CameraSystems.PerspectiveCamera.PerspectiveCamera;
using Unity.Entities;

#nullable enable
namespace DOTS.GamePlay.CameraSystems
{
    /// <summary>
    /// This system enables and disables the `PerspectiveCamera` that we instantiate in the `PerspectiveCameraInstantiateSystem`.
    /// </summary>
    public partial struct CameraSwitcherManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PerspectiveCam>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Whenever the state of the game changes between walking and rolling enable or disable the perspective camera.
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                if (gameState.ValueRO.State == GameState.Walking)
                {
                    var perspectiveCam = SystemAPI.ManagedAPI.GetSingleton<PerspectiveCam>();
                    if (perspectiveCam.camera != null)
                    {
                        perspectiveCam.camera.enabled = true;
                    }
                }
                if (gameState.ValueRO.State == GameState.Rolling)
                {
                    var perspectiveCam = SystemAPI.ManagedAPI.GetSingleton<PerspectiveCam>();
                    if (perspectiveCam.camera != null)
                    {
                        perspectiveCam.camera.enabled = false;
                    }
                }
            }
        }
    }
}

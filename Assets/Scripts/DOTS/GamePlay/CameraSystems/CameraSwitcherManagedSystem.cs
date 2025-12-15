using PerspectiveCameraObject = DOTS.GamePlay.CameraSystems.PerspectiveCamera.PerspectiveCameraObject;
using Unity.Entities;
using UnityEngine;
using DOTS.DataComponents;
using DOTS.GamePlay.CameraSystems.OrthographicCamera;

#nullable enable
namespace DOTS.GamePlay.CameraSystems
{
    /// <summary>
    /// This system enables and disables the `PerspectiveCamera` that we instantiate in the `PerspectiveCameraInstantiateSystem`.
    /// </summary>
    public partial struct CameraSwitcherManagedSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PerspectiveCameraObject>();
            state.RequireForUpdate<OrthographicCameraObject>();
            state.RequireForUpdate<GameStateComponent>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var orthoGraphicCamera = SystemAPI.ManagedAPI.GetSingleton<OrthographicCameraObject>();
            if (orthoGraphicCamera.camera == null)
            {
                Debug.LogWarning("[CameraSwitcherManagedSystem] OrthographicCamera not found");
            }
            state.EntityManager.CreateSingleton(new CurrentCameraManagedObject { Camera = orthoGraphicCamera.camera });
        }

        public void OnStopRunning(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                var currCamera = SystemAPI.ManagedAPI.GetSingleton<CurrentCameraManagedObject>();
                Camera? targetCamera; 

                targetCamera = gameState.ValueRO.State switch
                {
                    GameState.Walking => SystemAPI.ManagedAPI.GetSingleton<PerspectiveCameraObject>().camera,
                    GameState.Rolling => SystemAPI.ManagedAPI.GetSingleton<OrthographicCameraObject>().camera,
                    _ => null
                };

                if (targetCamera != null)
                {
                    SetActiveCamera(currCamera, targetCamera);
                }
            }
        }

        private void SetActiveCamera(CurrentCameraManagedObject current, Camera? target)
        {
            if (current.Camera == target) return;

            if (current.Camera != null)
            {
                current.Camera.enabled = false;
            }

            if (target != null)
            {
                current.Camera = target;
                current.Camera.enabled = true;
            }
        }
    }
}

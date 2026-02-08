using PerspectiveCameraObject = DOTS.GamePlay.CameraSystems.PerspectiveCamera.PerspectiveCameraObject;
using Unity.Entities;
using UnityEngine;
using DOTS.DataComponents;
using DOTS.GamePlay.CameraSystems.OrthographicCamera;
using Assets.Scripts.DOTS.GamePlay;

#nullable enable
namespace DOTS.GamePlay.CameraSystems
{
    /// <summary>
    /// This system enables and disables the `PerspectiveCamera` that we instantiate in the `PerspectiveCameraInstantiateSystem`.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
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
            var perspectiveCameraObject = SystemAPI.ManagedAPI.GetSingleton<PerspectiveCameraObject>();
            if (perspectiveCameraObject.camera == null)
            {
                Debug.LogWarning("[CameraSwitcherManagedSystem] PerspectiveCameraObject not found");
            }
            state.EntityManager.CreateSingleton(new CurrentCameraManagedObject { Camera = perspectiveCameraObject.camera });
        }

        public void OnStopRunning(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            //GameState should be a ghost component
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                var current = SystemAPI.ManagedAPI.GetSingleton<CurrentCameraManagedObject>();
                Camera? target;
                UnityEngine.Debug.Log($"[CameraSwitcherManagedSystem] | is this running? game state : {gameState.ValueRO.State.ToString()}");

                UnityEngine.Debug.Log($"[CameraSwitcherManagedSystem] | currCamera name: {current.Camera.name}");
                UnityEngine.Debug.Log($"[CameraSwitcherManagedSystem] | PerspectiveCamera name: {SystemAPI.ManagedAPI.GetSingleton<PerspectiveCameraObject>().camera.name}");
                UnityEngine.Debug.Log($"[CameraSwitcherManagedSystem] | orthoGraphicCamera name : {SystemAPI.ManagedAPI.GetSingleton<OrthographicCameraObject>().camera.name}");

                target = gameState.ValueRO.State switch
                {
                    GameState.Walking => SystemAPI.ManagedAPI.GetSingleton<PerspectiveCameraObject>().camera,
                    GameState.Rolling => SystemAPI.ManagedAPI.GetSingleton<OrthographicCameraObject>().camera,
                    _ => null
                };

                if (target != null)
                {
                    UnityEngine.Debug.Log($"[CameraSwitcherManagedSystem] | targetCamera is not null {target.name}");
                    SetActiveCamera(current, target);
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

using PerspectiveCameraObject = DOTS.GamePlay.CameraSystems.PerspectiveCamera.PerspectiveCameraObject;
using Unity.Entities;
using Unity.Mathematics;
using DOTS.GamePlay.CameraSystems.PerspectiveCamera;
using UnityEngine;
using DOTS.DataComponents;
using DOTS.GamePlay.CameraSystems.OrthographicCamera;

#nullable enable
namespace DOTS.GamePlay.CameraSystems
{
    /// <summary>
    /// This system enables and disables the `PerspectiveCamera` that we instantiate in the `PerspectiveCameraInstantiateSystem`.
    /// It also updates the current camera data.
    /// </summary>
    ///
    public struct CurrentCameraData : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
        public float3 LocalPosition;
        public quaternion LocalRotation;
        public float3 Offset;
        public float3 InitialAngle;
    }

    public partial struct CameraSwitcherManagedSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<CurrentCameraData>();
            state.RequireForUpdate<PerspectiveCameraObject>();
            state.RequireForUpdate<OrthographicCameraObject>();
            state.RequireForUpdate<CurrentCameraData>();
            state.RequireForUpdate<OrthoCamOffset>();
            state.RequireForUpdate<GameStateComponent>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentObject(entity, new CurrentCameraMangedObject { Camera = Camera.main });
        }

        public void OnStopRunning(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            // Whenever the state of the game changes between walking and rolling enable or disable the perspective camera.
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                ref var currCameraData = ref SystemAPI.GetSingletonRW<CurrentCameraData>().ValueRW;
                var currCamera = SystemAPI.ManagedAPI.GetSingleton<CurrentCameraMangedObject>();

                if (gameState.ValueRO.State == GameState.Walking)
                {
                    var perspectiveCam = SystemAPI.ManagedAPI.GetSingleton<PerspectiveCameraObject>();
                    var perspectiveCamConfig = SystemAPI.GetSingleton<PerspectiveCameraConfig>();

                    if (currCamera.Camera != null)
                    {
                        currCamera.Camera.enabled = false;
                    }

                    if (perspectiveCam.camera != null)
                    {
                        currCamera.Camera = perspectiveCam.camera;
                        currCamera.Camera.enabled = true;
                        SetCurrentCameraData(ref currCameraData, currCamera.Camera, perspectiveCamConfig.Offset, perspectiveCamConfig.Angle);
                    }
                }
                if (gameState.ValueRO.State == GameState.Rolling)
                {
                    var orthographicCam = SystemAPI.ManagedAPI.GetSingleton<OrthographicCameraObject>();
                    var orthoCamConfig = SystemAPI.GetSingleton<OrthoCamOffset>();

                    if (currCamera.Camera != null)
                    {
                        currCamera.Camera.enabled = false;
                    }

                    if (orthographicCam.Camera != null)
                    {
                        currCamera.Camera = orthographicCam.Camera;
                        currCamera.Camera.enabled = true;
                        SetCurrentCameraData(ref currCameraData, currCamera.Camera, orthoCamConfig.Offset, orthoCamConfig.Angle);
                    }
                }
            }
        }

        public void SetCurrentCameraData(ref CurrentCameraData currentCameraData, Camera newCameraData, float3 offset, float3 angle)
        {
            currentCameraData.Position = newCameraData.transform.position;
            currentCameraData.Rotation = newCameraData.transform.rotation;
            currentCameraData.LocalPosition = newCameraData.transform.localPosition;
            currentCameraData.LocalRotation = newCameraData.transform.localRotation;
            currentCameraData.Offset = offset;
            currentCameraData.InitialAngle = angle;
        }
    }
}

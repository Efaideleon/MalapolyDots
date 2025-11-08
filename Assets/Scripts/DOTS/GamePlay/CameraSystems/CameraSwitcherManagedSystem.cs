using PerspectiveCam = DOTS.GamePlay.CameraSystems.PerspectiveCamera.PerspectiveCamera;
using Unity.Entities;
using Unity.Mathematics;
using DOTS.GamePlay.CameraSystems.PerspectiveCamera;
using UnityEngine;

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

    public partial struct CameraSwitcherManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<CurrentCameraData>();
            state.RequireForUpdate<PerspectiveCam>();
            state.RequireForUpdate<CurrentCameraData>();
            state.RequireForUpdate<OrthoCamOffset>();
            state.RequireForUpdate<GameStateComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // Whenever the state of the game changes between walking and rolling enable or disable the perspective camera.
            foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
            {
                ref var currCameraData = ref SystemAPI.GetSingletonRW<CurrentCameraData>().ValueRW;
                if (gameState.ValueRO.State == GameState.Walking)
                {
                    var perspectiveCam = SystemAPI.ManagedAPI.GetSingleton<PerspectiveCam>();
                    var perspectiveCamConfig = SystemAPI.GetSingleton<PerspectiveCameraConfig>();
                    if (perspectiveCam.camera != null)
                    {
                        perspectiveCam.camera.enabled = true;
                        currCameraData.Position = perspectiveCam.camera.transform.position;
                        currCameraData.Rotation = perspectiveCam.camera.transform.rotation;
                        currCameraData.LocalPosition = perspectiveCam.camera.transform.localPosition;
                        currCameraData.LocalRotation = perspectiveCam.camera.transform.localRotation;
                        currCameraData.Offset = perspectiveCamConfig.Offset;
                        currCameraData.InitialAngle = perspectiveCamConfig.Angle;
                    }
                }
                if (gameState.ValueRO.State == GameState.Rolling)
                {
                    var perspectiveCam = SystemAPI.ManagedAPI.GetSingleton<PerspectiveCam>();
                    if (perspectiveCam.camera != null)
                    {
                        var orthoCamConfig = SystemAPI.GetSingleton<OrthoCamOffset>();
                        perspectiveCam.camera.enabled = false;
                        currCameraData.Position = Camera.main.transform.position;
                        currCameraData.Rotation = Camera.main.transform.rotation;
                        currCameraData.LocalPosition = Camera.main.transform.localPosition;
                        currCameraData.LocalRotation = Camera.main.transform.localRotation;
                        currCameraData.Offset = orthoCamConfig.Offset;
                        currCameraData.InitialAngle = orthoCamConfig.Angle;
                    }
                }
            }
        }
    }
}

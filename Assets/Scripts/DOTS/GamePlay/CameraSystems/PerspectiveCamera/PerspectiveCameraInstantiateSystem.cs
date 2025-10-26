using DOTS.DataComponents;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

#nullable enable
namespace DOTS.GamePlay.CameraSystems.PerspectiveCamera
{
    public partial struct PerspectiveCameraInstantiateSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PerspectiveCameraGO>();
            state.RequireForUpdate<PerspectiveCameraGOTag>();
            state.RequireForUpdate<PerspectiveCameraConfig>();
            state.RequireForUpdate<CurrentPlayerComponent>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var entity = SystemAPI.GetSingletonEntity<PerspectiveCameraGOTag>();
            var cameraGO = state.EntityManager.GetComponentObject<PerspectiveCameraGO>(entity);

            if (cameraGO == null) return;

            var camGO = GameObject.Instantiate(cameraGO.cameraGO);
            if (camGO == null) return;

            var cam = camGO.GetComponent<Camera>();

            // Set the camera initial position.
            var currentPlayer = SystemAPI.GetSingleton<CurrentPlayerComponent>();
            if (!SystemAPI.HasComponent<LocalTransform>(currentPlayer.entity))
                return;

            var player = SystemAPI.GetComponent<LocalTransform>(currentPlayer.entity);
            var camConfig = SystemAPI.GetSingleton<PerspectiveCameraConfig>();
            var newCamPosition =  player.Position + camConfig.Offset;

            float3 forward = math.normalize(player.Position - newCamPosition);
            var newCamRotation = quaternion.LookRotationSafe(forward, math.up());
             
            cam.transform.SetPositionAndRotation(newCamPosition, newCamRotation);

            state.EntityManager.CreateSingleton(new PerspectiveCamera { camera = cam });
        }

        public void OnUpdate(ref SystemState state)
        { }


        public void OnStopRunning(ref SystemState state)
        { }
    }

    public class PerspectiveCamera : IComponentData
    {
        public Camera? camera;
    }
}

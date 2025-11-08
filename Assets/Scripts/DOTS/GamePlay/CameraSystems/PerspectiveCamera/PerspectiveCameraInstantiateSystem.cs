using DOTS.DataComponents;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

#nullable enable
namespace DOTS.GamePlay.CameraSystems.PerspectiveCamera
{
    public partial struct PerspectiveCameraInstantiateSystem : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PerspectiveCameraPivotGO>();
            state.RequireForUpdate<PerspectiveCameraGOTag>();
            state.RequireForUpdate<PerspectiveCameraConfig>();
            state.RequireForUpdate<CurrentPlayerComponent>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            var entity = SystemAPI.GetSingletonEntity<PerspectiveCameraGOTag>();
            var cameraGO = state.EntityManager.GetComponentObject<PerspectiveCameraPivotGO>(entity);

            if (cameraGO == null) return;

            var camPivotGO = GameObject.Instantiate(cameraGO.Pivot);
            if (camPivotGO == null) return;

            state.EntityManager.CreateSingleton(new PerspectiveCameraPivot { Instance = camPivotGO });

            // Get the child components that has the Camera
            var cam = camPivotGO.GetComponentInChildren<Camera>();

            // Set the camera initial position with respect to the pivot.
            var origin = float3.zero;
            var camConfig = SystemAPI.GetSingleton<PerspectiveCameraConfig>();
            var newCamPosition =  origin + camConfig.Offset;

            float3 forward = math.normalize(origin - newCamPosition);
            var newCamRotation = quaternion.LookRotationSafe(forward, math.up());
             
            cam.transform.SetLocalPositionAndRotation(newCamPosition, newCamRotation);

            state.EntityManager.CreateSingleton(new PerspectiveCamera { camera = cam });
        }

        public void OnUpdate(ref SystemState state)
        { }


        public void OnStopRunning(ref SystemState state)
        { }
    }

    public class PerspectiveCameraPivot : IComponentData
    {
        public GameObject? Instance;
    }

    public class PerspectiveCamera : IComponentData
    {
        public Camera? camera;
    }
}

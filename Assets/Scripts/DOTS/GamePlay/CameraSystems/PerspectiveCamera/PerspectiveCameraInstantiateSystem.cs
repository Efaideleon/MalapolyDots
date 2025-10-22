using Unity.Entities;
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
        }

        public void OnStartRunning(ref SystemState state)
        {
            var entity = SystemAPI.GetSingletonEntity<PerspectiveCameraGOTag>();
            var cameraGO = state.EntityManager.GetComponentObject<PerspectiveCameraGO>(entity);

            if (cameraGO == null) return;

            var camGO = GameObject.Instantiate(cameraGO.cameraGO);
            if (camGO == null) return;

            var cam = camGO.GetComponent<Camera>();

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

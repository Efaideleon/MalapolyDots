using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems.OrthographicCamera
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct InstantiateOrthographicCameraPivot : ISystem, ISystemStartStop
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<OrthographicCameraPivot>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            if (SystemAPI.ManagedAPI.HasSingleton<OrthoCameraPivotInstance>()) return;

            var orthographicCameraPivot = SystemAPI.ManagedAPI.GetSingleton<OrthographicCameraPivot>();
            if (orthographicCameraPivot.gameObject == null) return;

            var pivotGO = GameObject.Instantiate(orthographicCameraPivot.gameObject, float3.zero, quaternion.identity);
            // Get the child of this object (the camera) and load into the camera component?

            if (pivotGO == null) return;

            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<OrthoCameraPivotInstance>(),
                ComponentType.ReadOnly<OrthoCameraPivotInstanceTag>(),
            });

            UnityEngine.Debug.Log($"[InstantiateOrthographicCameraPivot] | pivot position: {pivotGO.transform.position}");
            UnityEngine.Debug.Log($"[InstantiateOrthographicCameraPivot] | pivot rotation: {pivotGO.transform.rotation}");
            state.EntityManager.SetComponentData(entity, new OrthoCameraPivotInstance { Instance = pivotGO });
            SystemAPI.SetComponent(entity, new OrthoCameraPivotInstanceTag { });

            // Get the child components that has the Camera
            var cam = pivotGO.GetComponentInChildren<Camera>();

            // Set the camera initial position with respect to the pivot.
            var camConfig = SystemAPI.GetSingleton<OrthoCamOffset>();

            var player = new float3(0, 0, 0);
            var newCamPosition = player + camConfig.Offset;

            float3 forward = math.normalize(player - newCamPosition);
            var newCamRotation = quaternion.LookRotationSafe(forward, math.up());

            var camFieldOfView = SystemAPI.GetSingleton<CameraFieldOfView>();
            cam.transform.SetLocalPositionAndRotation(newCamPosition, newCamRotation);
            cam.orthographic = true;
            cam.orthographicSize = camFieldOfView.Value;

            var cameObjEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentObject(cameObjEntity, new OrthographicCameraObject { camera = cam });
            UnityEngine.Debug.Log($"[InstantiateOrthographicCameraPivot] | Instantiating orthogrphic camera pivot");
        }

        public void OnUpdate(ref SystemState state)
        { }

        public void OnStopRunning(ref SystemState state)
        { }
    }

    public class OrthoCameraPivotInstance : IComponentData
    {
        public GameObject Instance;
    }

    public struct OrthoCameraPivotInstanceTag : IComponentData
    { }
}

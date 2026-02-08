using Assets.Scripts.DOTS.GamePlay;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems.OrthographicCamera
{
    /// <summary> Store a reference to the orthographic camera managed object. </summary>
    public class OrthographicCameraObject : IComponentData
    {
        public Camera? camera;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
    public partial struct InstantiateOrthographicCameraPivot : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<OrthographicCameraPivot>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GhostDataLoadedTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.ManagedAPI.HasSingleton<OrthoCameraPivotInstance>())
            {
                state.Enabled = false;
                UnityEngine.Debug.Log($"[InstantiateOrthographicCameraPivot] | instantiated");
                return;
            }

            var activePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>();
            if (activePlayer.Entity == Entity.Null)
            {
                UnityEngine.Debug.Log($"[InstantiateOrthographicCameraPivot] | player is null");
                return;
            }

            if (!SystemAPI.HasComponent<LocalTransform>(activePlayer.Entity))
            {
                UnityEngine.Debug.Log($"[InstantiateOrthographicCameraPivot] | no local transform");
                return;
            }

            var orthographicCameraPivot = SystemAPI.ManagedAPI.GetSingleton<OrthographicCameraPivot>();
            if (orthographicCameraPivot.gameObject == null) return;

            var playerPosition = SystemAPI.GetComponent<LocalToWorld>(activePlayer.Entity).Position;
            var pivotGO = GameObject.Instantiate(orthographicCameraPivot.gameObject, playerPosition, quaternion.identity);
            // Get the child of this object (the camera) and load into the camera component?

            if (pivotGO == null) return;

            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<OrthoCameraPivotInstance>(),
                ComponentType.ReadOnly<OrthoCameraPivotInstanceTag>(),
            });

            state.EntityManager.SetComponentData(entity, new OrthoCameraPivotInstance { Instance = pivotGO });
            SystemAPI.SetComponent(entity, new OrthoCameraPivotInstanceTag { });

            // Get the child components that has the Camera
            var cam = pivotGO.GetComponentInChildren<Camera>();

            // Set the camera initial position with respect to the pivot.
            var camConfig = SystemAPI.GetSingleton<OrthoCamOffset>();

            var localCamPos = camConfig.Offset;

            float3 directionToPivot = math.normalize(-localCamPos);
            var localCamRotation = quaternion.LookRotationSafe(directionToPivot, math.up());

            var camFieldOfView = SystemAPI.GetSingleton<CameraFieldOfView>();
            cam.transform.SetLocalPositionAndRotation(localCamPos, localCamRotation);
            cam.orthographic = true;
            cam.orthographicSize = camFieldOfView.Value;

            var cameObjEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentObject(cameObjEntity, new OrthographicCameraObject { camera = cam });
            UnityEngine.Debug.Log($"[InstantiateOrthographicCameraPivot] | Instantiating orthogrphic camera pivot");
        }
    }

    public class OrthoCameraPivotInstance : IComponentData
    {
        public GameObject Instance;
    }

    public struct OrthoCameraPivotInstanceTag : IComponentData
    { }
}

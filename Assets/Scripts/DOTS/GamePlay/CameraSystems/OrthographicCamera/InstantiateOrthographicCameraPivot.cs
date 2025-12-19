using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems.OrthographicCamera
{
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

            state.EntityManager.SetComponentData(entity, new OrthoCameraPivotInstance { Instance = pivotGO });
            SystemAPI.SetComponent(entity, new OrthoCameraPivotInstanceTag {});
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

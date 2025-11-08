using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

#nullable enable
namespace DOTS.GamePlay.CameraSystems.PerspectiveCamera
{
    public class PerspectiveCameraAuthoring : MonoBehaviour
    {
        [SerializeField] GameObject? PerspectiveCameraPivotGO;
        [SerializeField] float3 offset;
        [SerializeField] float angle;

        public class PerspectiveCameraBaker : Baker<PerspectiveCameraAuthoring>
        {
            public override void Bake(PerspectiveCameraAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new PerspectiveCameraPivotGO { Pivot = authoring.PerspectiveCameraPivotGO });
                AddComponent(entity, new PerspectiveCameraGOTag { });
                AddComponent(entity, new PerspectiveCameraConfig { Offset = authoring.offset, Angle = authoring.angle });
            }
        }
    }

    public struct PerspectiveCameraGOTag : IComponentData
    {
    }

    public class PerspectiveCameraPivotGO : IComponentData
    {
        public GameObject? Pivot;
    }

    public struct PerspectiveCameraConfig : IComponentData
    {
        public float3 Offset;
        public float Angle;
    }
}

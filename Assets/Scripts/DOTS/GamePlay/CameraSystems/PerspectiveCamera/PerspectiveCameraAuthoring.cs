using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

#nullable enable
namespace DOTS.GamePlay.CameraSystems.PerspectiveCamera
{
    public class PerspectiveCameraAuthoring : MonoBehaviour
    {
        [SerializeField] GameObject? perspectiveCameraGO;
        [SerializeField] float3 offset;
        [SerializeField] float angle;

        public class PerspectiveCameraBaker : Baker<PerspectiveCameraAuthoring>
        {
            public override void Bake(PerspectiveCameraAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new PerspectiveCameraGO { cameraGO = authoring.perspectiveCameraGO });
                AddComponent(entity, new PerspectiveCameraGOTag { });
                AddComponent(entity, new PerspectiveCameraConfig { Offset = authoring.offset, Angle = authoring.angle });
            }
        }
    }

    public struct PerspectiveCameraGOTag : IComponentData
    {
    }

    public class PerspectiveCameraGO : IComponentData
    {
        public GameObject? cameraGO;
    }

    public struct PerspectiveCameraConfig : IComponentData
    {
        public float3 Offset;
        public float Angle;
    }
}

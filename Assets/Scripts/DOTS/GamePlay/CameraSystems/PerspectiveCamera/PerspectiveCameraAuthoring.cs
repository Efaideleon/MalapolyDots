using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

#nullable enable
namespace DOTS.GamePlay.CameraSystems.PerspectiveCamera
{
    public class PerspectiveCameraAuthoring : MonoBehaviour
    {
        [Header("Pivot")]
        [SerializeField] GameObject? PerspectiveCameraPivotGO;
        [Header("Perspective Camera Config")]
        [SerializeField] CameraConfig? cameraConfig;

        public class PerspectiveCameraBaker : Baker<PerspectiveCameraAuthoring>
        {
            public override void Bake(PerspectiveCameraAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new PerspectiveCameraPivotGO { Pivot = authoring.PerspectiveCameraPivotGO });
                AddComponent(entity, new PerspectiveCameraGOTag { });
                if (authoring.cameraConfig != null)
                {
                    AddComponent(entity, new PerspectiveCameraConfig
                    {
                        Offset = authoring.cameraConfig.offset,
                        Angle = authoring.cameraConfig.angle
                    });
                }
            }
        }
    }

    public struct PerspectiveCameraGOTag : IComponentData { }

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

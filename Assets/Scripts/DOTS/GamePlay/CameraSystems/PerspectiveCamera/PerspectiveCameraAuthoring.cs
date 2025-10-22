using UnityEngine;
using Unity.Entities;

#nullable enable
namespace DOTS.GamePlay.CameraSystems.PerspectiveCamera
{
    public class PerspectiveCameraAuthoring : MonoBehaviour
    {
        [SerializeField] GameObject? perspectiveCameraGO;

        public class PerspectiveCameraBaker : Baker<PerspectiveCameraAuthoring>
        {
            public override void Bake(PerspectiveCameraAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponentObject(entity, new PerspectiveCameraGO { cameraGO = authoring.perspectiveCameraGO });
                AddComponent(entity, new PerspectiveCameraGOTag {});
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
}

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// ============================================================================================
// This system initializes the components for camera properties to be used by others DOTS systems
// It does NOT get the a reference to the actual Camera in the scene.
// ============================================================================================
namespace DOTS.GamePlay.CameraSystems
{
    public struct MainCameraTransform : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }

    public struct OrthoCamOffset : IComponentData
    {
        public float3 Offset;
        public float Angle;
    }

    public struct MainCameraTranslateData : IComponentData
    {
        public float3 Delta;
    }

    public struct CameraFieldOfView : IComponentData
    {
        public float Value;
    }

    public class OrthographicCameraPivot : IComponentData
    {
        public GameObject gameObject;
    }

    public class OrthoCameraSetupAuthoring : MonoBehaviour
    {
        [Header("Pivot")]
        [SerializeField] GameObject OrthographicCameraPivotGO;
        [Header("Camera")]
        public Transform CameraTransform;
        [Header("Perspective Camera Config")]
        public CameraConfig cameraConfig;

        public class CameraBaker : Baker<OrthoCameraSetupAuthoring>
        {
            public override void Bake(OrthoCameraSetupAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponentObject(entity, new OrthographicCameraPivot { gameObject = authoring.OrthographicCameraPivotGO });

                AddComponent(entity, new MainCameraTransform
                {
                    Position = authoring.CameraTransform.position,
                    Rotation = authoring.CameraTransform.rotation
                });

                AddComponent(entity, new MainCameraTranslateData { Delta = default, });
                AddComponent(entity, new CameraFieldOfView { Value = authoring.cameraConfig.fieldOfView });
                AddComponent(entity, new OrthoCamOffset
                {
                    Offset = authoring.cameraConfig.offset,
                    Angle = math.radians(authoring.cameraConfig.angle)
                });
            }
        }
    }
}

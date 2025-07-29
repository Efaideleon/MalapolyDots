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

    public struct MainCameraTranslateData : IComponentData
    {
        public float3 Delta;
    }

    public struct CameraFieldOfView : IComponentData
    {
        public float Value;
    }

    public class CameraSetupAuthoring : MonoBehaviour
    {
        public Transform CameraTransform;

        public class CameraBaker : Baker<CameraSetupAuthoring>
        {
            public override void Bake(CameraSetupAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                AddComponent(entity, new MainCameraTransform 
                {
                    Position = authoring.CameraTransform.position,
                    Rotation = authoring.CameraTransform.rotation
                });

                AddComponent(entity, new MainCameraTranslateData 
                {
                    Delta = default,
                });

                AddComponent(entity, new CameraFieldOfView 
                {
                    Value = default, 
                });
            }
        }
    }
}

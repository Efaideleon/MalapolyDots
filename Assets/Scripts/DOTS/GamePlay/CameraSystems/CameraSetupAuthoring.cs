using log4net.Repository.Hierarchy;
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

    public struct CameraOffset : IComponentData
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

    public class CameraSetupAuthoring : MonoBehaviour
    {
        public Transform CameraTransform;
        public float3 Offset = new (0f, 13f, 27f);
        public float Angle = 51; // Should limit values between 0 and 360 in inspector.

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

                AddComponent(entity, new CameraOffset 
                {
                    Offset = authoring.Offset, 
                    Angle = math.radians(authoring.Angle)
                });
            }
        }
    }
}

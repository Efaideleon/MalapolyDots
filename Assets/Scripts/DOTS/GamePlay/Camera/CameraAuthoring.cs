using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.GamePlay.Camera
{
    public class CameraAuthoring : MonoBehaviour
    {
        public class CameraBaker : Baker<CameraAuthoring>
        {
            public override void Bake(CameraAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new CameraFollow 
                {
                    Target = Entity.Null,
                    Offset = default,
                    SmoothSpeed = default
                });
            }
        }
    }

    public struct CameraFollow : IComponentData
    {
        public Entity Target;
        public float3 Offset;
        public float SmoothSpeed;
    }
}

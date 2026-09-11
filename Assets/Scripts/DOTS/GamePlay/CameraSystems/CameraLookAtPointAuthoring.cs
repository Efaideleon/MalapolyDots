using Unity.Entities;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems
{
    public class CameraLookAtPointAuthoring : MonoBehaviour
    {
        public class CameraLookAtPointBaker : Baker<CameraLookAtPointAuthoring>
        {
            public override void Bake(CameraLookAtPointAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent<CameraLookAtPointTag>(entity);
            }
        }
    }

    public struct CameraLookAtPointTag : IComponentData
    {}
}

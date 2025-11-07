using Unity.Entities;
using UnityEngine;

namespace DOTS.GamePlay.CameraSystems
{
    public class CameraSceneAuthoring : MonoBehaviour
    {
        public class CameraSceneBaker : Baker<CameraSceneAuthoring>
        {
            public override void Bake(CameraSceneAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new CameraSceneTag { });
            }
        }
    }

    public struct CameraSceneTag : IComponentData
    { }
}

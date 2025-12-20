using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GamePlay.LandingPropertyIcons
{
    public class LandingPropertyIconAuthoring : MonoBehaviour
    {
        public class LandingPropertyIconBaker : Baker<LandingPropertyIconAuthoring>
        {
            public override void Bake(LandingPropertyIconAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new OwnerIconTag { });
                AddComponent(entity, new UVOffsetOverride { });
                AddComponent(entity, new UVScaleOverride { });
            }
        }
    }

    public struct OwnerIconTag : IComponentData
    { }
}

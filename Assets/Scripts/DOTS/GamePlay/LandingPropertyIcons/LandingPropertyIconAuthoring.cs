using DOTS.DataComponents;
using Unity.Entities;
using Unity.Mathematics;
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
                AddComponent(entity, new UVScaleOverride { Value = new float2(1f, 1f) });
            }
        }
    }

    public struct OwnerIconTag : IComponentData
    { }
}

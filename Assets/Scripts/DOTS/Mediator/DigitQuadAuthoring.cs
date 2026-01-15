using DOTS.DataComponents;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Mediator
{
    public class DigitQuadAuthoring : MonoBehaviour
    {
        public class DigitQuadBaker : Baker<DigitQuadAuthoring>
        {
            public override void Bake(DigitQuadAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                //AddComponent<UVOffsetOverride>(entity);
                AddComponent(entity, new UVScaleOverride { Value = new float2(0.25f, 0.25f)});
                AddComponent(entity, new UVOffsetOverride { Value = default });
            }
        }
    }
}

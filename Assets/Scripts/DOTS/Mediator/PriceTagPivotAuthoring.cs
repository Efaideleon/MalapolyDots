using Unity.Entities;
using UnityEngine;

namespace DOTS.Mediator
{
    public class PriceTagPivotAuthoring : MonoBehaviour
    {
        public class PriceTagPivotBaker : Baker<PriceTagPivotAuthoring>
        {
            public override void Bake(PriceTagPivotAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent<PriceTagPivotTag>(entity);
            }
        }
    }

    public struct PriceTagPivotTag : IComponentData
    { }
}

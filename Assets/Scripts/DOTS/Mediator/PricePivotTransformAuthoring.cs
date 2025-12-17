using Unity.Entities;
using UnityEngine;

namespace DOTS.Mediator
{
    public class PricePivotTransformAuthoring : MonoBehaviour
    {
        public class PricePivotTransformBaker : Baker<PricePivotTransformAuthoring>
        {
            public override void Bake(PricePivotTransformAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent<PricePivotTransformTag>(entity);
            }
        }
    }

    public struct PricePivotTransformTag : IComponentData
    { }
}

using DOTS.GameSpaces;
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
                // Static baking can remove Transform Parent; retain the owning property explicitly.
                var property = GetComponentInParent<PropertySpaceAuthoring>();
                if (property != null)
                    AddComponent(entity, new PriceTagProperty
                    {
                        Value = GetEntity(property, TransformUsageFlags.Dynamic)
                    });
            }
        }
    }

    public struct PriceTagPivotTag : IComponentData
    { }

    public struct PriceTagProperty : IComponentData
    {
        public Entity Value;
    }

    public struct PriceTagSpawned : IComponentData
    { }
}

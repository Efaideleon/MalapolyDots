using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.Mediator.PriceTag.Authoring
{
    public class PriceTagAuthoring : MonoBehaviour
    {
        public class PriceTagBaker : Baker<PriceTagAuthoring>
        {
            public override void Bake(PriceTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent<PriceTagTag>(entity);
                AddComponent<NumberID>(entity);
            }
        }
    }

    public struct PriceTagTag : IComponentData
    { }

    public struct NumberID : IComponentData
    {
        public int Value;
    }
}

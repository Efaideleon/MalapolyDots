using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.Mediator.PriceTag.Authoring
{
    public class PriceTagLoaderAuthoring : MonoBehaviour
    {
        [SerializeField] GameObject PriceTagPrefab;

        public class PriceTagLoaderBaker : Baker<PriceTagLoaderAuthoring>
        {
            public override void Bake(PriceTagLoaderAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new PriceTagReference { Entity = GetEntity(authoring.PriceTagPrefab, TransformUsageFlags.Dynamic) });
            }
        }
    }

    public struct PriceTagReference : IComponentData
    {
        public Entity Entity;
    }
}


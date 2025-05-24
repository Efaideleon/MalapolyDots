using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class ForSaleSignAuthoring : MonoBehaviour
    {
        [SerializeField] GameObject prefab;

        public class ForSaleSignBaker : Baker<ForSaleSignAuthoring>
        {
            public override void Bake(ForSaleSignAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                var PrefabEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new ForSaleSignTag { });
            }
        }
    }

    public struct ForSaleSignTag : IComponentData { }
}

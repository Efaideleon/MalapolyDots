using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharactersTags
{
    public class CoffeeMaterialTagAuthoring : MonoBehaviour
    {
        public class CoffeeMaterialTagBaker : Baker<CoffeeMaterialTagAuthoring>
        {
            public override void Bake(CoffeeMaterialTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new CoffeeMaterialTag {});
            }
        }
    }

    public struct CoffeeMaterialTag : IComponentData { }
}

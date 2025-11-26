using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharacterTags
{
    public class CoffeeCharacterTagAuthoring : MonoBehaviour
    {
        class CoffeeTagBaker : Baker<CoffeeCharacterTagAuthoring>
        {
            public override void Bake(CoffeeCharacterTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new CoffeeMaterialTag { });
            }
        }
    }

    public struct CoffeeMaterialTag : IComponentData { }
}

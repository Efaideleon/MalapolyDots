using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharacterTags
{
    public class TuctucCharacterTagAuthoring : MonoBehaviour
    {
        class TuctucTagBaker : Baker<TuctucCharacterTagAuthoring>
        {
            public override void Bake(TuctucCharacterTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new TuctucMaterialTag { });
            }
        }
    }

    public struct TuctucMaterialTag : IComponentData { }
}


using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharacterTags
{
    public class AvocadoCharacterTagAuthoring : MonoBehaviour
    {
        class AvocadoTagBaker : Baker<AvocadoCharacterTagAuthoring>
        {
            public override void Bake(AvocadoCharacterTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new AvocadoMaterialTag { });
            }
        }
    }

    public struct AvocadoMaterialTag : IComponentData { }
}

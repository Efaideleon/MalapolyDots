using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharacterTags
{
    public class BirdCharacterTagAuthoring : MonoBehaviour
    {
        class BirdTagBaker : Baker<BirdCharacterTagAuthoring>
        {
            public override void Bake(BirdCharacterTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new BirdMaterialTag { });
            }
        }
    }

    public struct BirdMaterialTag : IComponentData { }
}

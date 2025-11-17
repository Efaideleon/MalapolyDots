using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharactersTags
{
    public class BirdMaterialTagAuthoring : MonoBehaviour
    {
        public class BirdMaterialTagBaker : Baker<BirdMaterialTagAuthoring>
        {
            public override void Bake(BirdMaterialTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new BirdMaterialTag {});
            }
        }
    }

    public struct BirdMaterialTag : IComponentData { }
}

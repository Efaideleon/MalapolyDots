using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharactersTags
{
    public class TuctucMaterialTagAuthoring : MonoBehaviour
    {
        public class TuctucMaterialTagBaker : Baker<TuctucMaterialTagAuthoring>
        {
            public override void Bake(TuctucMaterialTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new TuctucMaterialTag {});
            }
        }
    }

    public struct TuctucMaterialTag : IComponentData { }
}

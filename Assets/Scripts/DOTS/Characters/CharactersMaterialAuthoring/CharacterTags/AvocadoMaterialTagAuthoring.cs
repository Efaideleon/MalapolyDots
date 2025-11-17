using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharactersTags
{
    public class AvocadoMaterialTagAuthoring : MonoBehaviour
    {
        public class AvocadoMaterialTagBaker : Baker<AvocadoMaterialTagAuthoring>
        {
            public override void Bake(AvocadoMaterialTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new AvocadoMaterialTag {});
            }
        }
    }

    public struct AvocadoMaterialTag : IComponentData { }
}

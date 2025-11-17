using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharactersTags
{
    public class LiraMaterialTagAuthoring : MonoBehaviour
    {
        public class LiraMaterialTagBaker : Baker<LiraMaterialTagAuthoring>
        {
            public override void Bake(LiraMaterialTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new LiraMaterialTag {});
            }
        }
    }

    public struct LiraMaterialTag : IComponentData { }
}

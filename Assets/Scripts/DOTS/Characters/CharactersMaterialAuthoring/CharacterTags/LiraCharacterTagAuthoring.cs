using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharacterTags
{
    public class LiraCharacterTagAuthoring : MonoBehaviour
    {
        class LiraTagBaker : Baker<LiraCharacterTagAuthoring>
        {
            public override void Bake(LiraCharacterTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new LiraMaterialTag { });
            }
        }
    }

    public struct LiraMaterialTag : IComponentData { }
}


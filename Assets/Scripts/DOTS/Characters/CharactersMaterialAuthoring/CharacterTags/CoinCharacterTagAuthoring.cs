using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharacterTags
{
    public class CoinCharacterTagAuthoring : MonoBehaviour
    {
        class CoinTagBaker : Baker<CoinCharacterTagAuthoring>
        {
            public override void Bake(CoinCharacterTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new CoinMaterialTag { });
            }
        }
    }

    public struct CoinMaterialTag : IComponentData { }
}


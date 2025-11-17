using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring.CharactersTags
{
    public class CoinMaterialTagAuthoring : MonoBehaviour
    {
        public class CoinMaterialTagBaker : Baker<CoinMaterialTagAuthoring>
        {
            public override void Bake(CoinMaterialTagAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new CoinMaterialTag {});
            }
        }
    }

    public struct CoinMaterialTag : IComponentData { }
}

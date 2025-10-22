using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class TreasureSpaceAuthoring : MonoBehaviour
    {
        public SpaceProperties Space;

        class TreasureSpaceAuthoringBaker : Baker<TreasureSpaceAuthoring>
        {
            public override void Bake(TreasureSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Space.Name });
                AddComponent(entity, new SpaceIDComponent { Value = default });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new TreasureSpaceTag {});
                AddComponent(entity, new SpaceTypeComponent{ Value = SpaceType.Treasure });
            }
        }
    }

    public struct TreasureSpaceTag : IComponentData
    { }
}

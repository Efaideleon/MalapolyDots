using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class GoSpaceAuthoring : MonoBehaviour
    {
        public SpaceProperties Space;

        class GoSpaceAuthoringBaker : Baker<GoSpaceAuthoring>
        {
            public override void Bake(GoSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Space.Name });
                AddComponent(entity, new SpaceIDComponent { Value = default });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new GoSpaceTag { });
                AddComponent(entity, new SpaceTypeComponent{ Value = SpaceType.Go });
            }
        }
    }

    public struct GoSpaceTag : IComponentData
    { }
}

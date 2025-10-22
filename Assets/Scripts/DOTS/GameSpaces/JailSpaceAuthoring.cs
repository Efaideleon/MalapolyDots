using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class JailSpaceAuthoring : MonoBehaviour
    {
        public SpaceProperties Space;

        class JailSpaceAuthoringBaker : Baker<JailSpaceAuthoring>
        {
            public override void Bake(JailSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Space.Name });
                AddComponent(entity, new SpaceIDComponent { Value = default });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new JailSpaceTag { });
                AddComponent(entity, new SpaceTypeComponent{ Value = SpaceType.Jail });
            }
        }
    }

    public struct JailSpaceTag : IComponentData
    { }
}

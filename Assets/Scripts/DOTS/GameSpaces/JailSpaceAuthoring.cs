using DOTS.DataComponents;
using DOTS.GameData.PlacesData;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class JailSpaceAuthoring : MonoBehaviour
    {
        public JailSpaceData Data;

        class JailSpaceAuthoringBaker : Baker<JailSpaceAuthoring>
        {
            public override void Bake(JailSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Data.Name });
                AddComponent(entity, new SpaceIDComponent { Value = authoring.Data.id });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new JailSpaceTag { });
                AddComponent(entity, new SpaceTypeComponent{ Value = authoring.Data.SpaceType });
            }
        }
    }

    public struct JailSpaceTag : IComponentData
    { }
}

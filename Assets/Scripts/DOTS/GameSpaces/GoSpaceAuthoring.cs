using DOTS.DataComponents;
using DOTS.GameData.PlacesData;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class GoSpaceAuthoring : MonoBehaviour
    {
        public GoSpaceData Data;

        class GoSpaceAuthoringBaker : Baker<GoSpaceAuthoring>
        {
            public override void Bake(GoSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Data.Name });
                AddComponent(entity, new SpaceIDComponent { Value = authoring.Data.id });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new GoSpaceTag { });
                AddComponent(entity, new SpaceTypeComponent{ Value = authoring.Data.SpaceType });
            }
        }
    }

    public struct GoSpaceTag : IComponentData
    { }
}

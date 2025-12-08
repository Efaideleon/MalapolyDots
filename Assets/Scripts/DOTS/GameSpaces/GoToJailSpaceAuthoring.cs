using DOTS.DataComponents;
using DOTS.GameData.PlacesData;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class GoToJailSpaceAuthoring : MonoBehaviour
    {
        public GoToJailSpaceData Data;

        class GoToJailSpaceAuthoringBaker : Baker<GoToJailSpaceAuthoring>
        {
            public override void Bake(GoToJailSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Data.Name });
                AddComponent(entity, new SpaceIDComponent { Value = authoring.Data.id });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new GoToJailTag { });
                AddComponent(entity, new SpaceTypeComponent{ Value = authoring.Data.SpaceType });
            }
        }
    }

    public struct GoToJailTag : IComponentData
    { }

}

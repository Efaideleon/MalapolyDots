using DOTS.DataComponents;
using DOTS.GameData.PlacesData;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class ParkingSpaceAuthoring : MonoBehaviour
    {
        public ParkingSpaceData Data;

        class ParkingSpaceAuthoringBaker : Baker<ParkingSpaceAuthoring>
        {
            public override void Bake(ParkingSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Data.Name });
                AddComponent(entity, new SpaceIDComponent { Value = authoring.Data.id });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new ParkingSpaceTag { });
                AddComponent(entity, new SpaceTypeComponent{ Value = authoring.Data.SpaceType });
            }
        }
    }

    public struct ParkingSpaceTag : IComponentData
    { }
}

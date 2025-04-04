using Unity.Entities;
using UnityEngine;

public class ParkingSpaceAuthoring : MonoBehaviour
{
    [SerializeField] string Name;

    class ParkingSpaceAuthoringBaker : Baker<ParkingSpaceAuthoring>
    {
        public override void Bake(ParkingSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = default });
            AddComponent(entity, new BoardIndexComponent { Value = default });
            AddComponent(entity, new ParkingSpaceTag { });
            AddComponent(entity, new SpaceTypeComponent{ Value = SpaceTypeEnum.Parking });
        }
    }
}

public struct ParkingSpaceTag : IComponentData
{ }

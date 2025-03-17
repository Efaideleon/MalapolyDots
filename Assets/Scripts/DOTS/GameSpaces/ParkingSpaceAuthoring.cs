using Unity.Entities;
using UnityEngine;

public class ParkingSpaceAuthoring : MonoBehaviour
{
    [SerializeField] ParkingSpaceData data;

    class ParkingSpaceAuthoringBaker : Baker<ParkingSpaceAuthoring>
    {
        public override void Bake(ParkingSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.data.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.data.id });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.data.boardIndex });
            AddComponent(entity, new ParkingSpaceTag { });
        }
    }
}

public struct ParkingSpaceTag : IComponentData
{ }

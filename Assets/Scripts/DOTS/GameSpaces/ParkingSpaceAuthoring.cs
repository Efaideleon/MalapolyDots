using Unity.Entities;
using UnityEngine;

public class ParkingSpaceAuthoring : MonoBehaviour
{
    [SerializeField] int ID;
    [SerializeField] string Name;
    [SerializeField] int BoardIndex;

    class ParkingSpaceAuthoringBaker : Baker<ParkingSpaceAuthoring>
    {
        public override void Bake(ParkingSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.ID });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.BoardIndex });
            AddComponent(entity, new ParkingSpaceTag { });
        }
    }
}

public struct ParkingSpaceTag : IComponentData
{ }

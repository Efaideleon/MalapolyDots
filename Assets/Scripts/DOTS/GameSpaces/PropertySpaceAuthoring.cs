using Unity.Entities;
using UnityEngine;

public class PropertySpaceAuthoring : MonoBehaviour
{
    [SerializeField] string Name;

    class PropertySpaceAuthoringBaker : Baker<PropertySpaceAuthoring>
    {
        public override void Bake(PropertySpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = default });
            AddComponent(entity, new BoardIndexComponent { Value = default });
            AddComponent(entity, new PriceComponent { Value = default });
            AddComponent(entity, new SpaceTypeComponent { Value = default });
            AddComponent(entity, new OwnerComponent { ID = PropertyConstants.Vacant });
            AddComponent(entity, new PropertySpaceTag {});
            AddComponent(entity, new RentComponent { Value = default });
            AddBuffer<BaseRentBuffer>(entity);
        }
    }
}

public struct PropertySpaceTag : IComponentData
{ }


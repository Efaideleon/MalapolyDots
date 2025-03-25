using Unity.Entities;
using UnityEngine;

public class PropertySpaceAuthoring : MonoBehaviour
{
    [SerializeField] PropertySpaceData data;

    class PropertySpaceAuthoringBaker : Baker<PropertySpaceAuthoring>
    {
        public override void Bake(PropertySpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.data.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.data.id });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.data.boardIndex });
            AddComponent(entity, new SpacePriceComponent { Value = authoring.data.price });
            AddComponent(entity, new PropertySpaceTag {});
            AddComponent(entity, new SpaceTypeComponent{ Value = SpaceTypeEnum.Property });
            AddComponent(entity, new OwnerComponent{ OwnerID = -1 });
        }
    }
}

public struct PropertySpaceTag : IComponentData
{ }


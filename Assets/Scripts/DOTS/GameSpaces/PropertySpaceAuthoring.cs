using Unity.Entities;
using UnityEngine;

public class PropertySpaceAuthoring : MonoBehaviour
{
    [SerializeField] int ID;
    [SerializeField] string Name;
    [SerializeField] int BoardIndex;
    [SerializeField] int Price;

    class PropertySpaceAuthoringBaker : Baker<PropertySpaceAuthoring>
    {
        public override void Bake(PropertySpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.ID });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.BoardIndex });
            AddComponent(entity, new SpacePriceComponent { Value = authoring.Price });
            AddComponent(entity, new PropertySpaceTag {});
        }
    }
}

public struct PropertySpaceTag : IComponentData
{ }

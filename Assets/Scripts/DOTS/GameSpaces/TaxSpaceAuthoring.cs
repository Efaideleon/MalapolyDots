using Unity.Entities;
using UnityEngine;

public class TaxSpaceAuthoring : MonoBehaviour
{
    [SerializeField] string Name;

    class TaxSpaceAuthoringBaker : Baker<TaxSpaceAuthoring>
    {
        public override void Bake(TaxSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = default });
            AddComponent(entity, new BoardIndexComponent { Value = default });
            AddComponent(entity, new TaxSpaceTag { });
            AddComponent(entity, new SpaceTypeComponent{ Value = SpaceTypeEnum.Tax });
        }
    }
}

public struct TaxSpaceTag : IComponentData
{ }

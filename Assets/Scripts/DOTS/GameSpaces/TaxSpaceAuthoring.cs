using Unity.Entities;
using UnityEngine;

public class TaxSpaceAuthoring : MonoBehaviour
{
    [SerializeField] TaxSpaceData data;

    class TaxSpaceAuthoringBaker : Baker<TaxSpaceAuthoring>
    {
        public override void Bake(TaxSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.data.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.data.id });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.data.boardIndex });
            AddComponent(entity, new TaxSpaceTag { });
            AddComponent(entity, new SpaceTypeComponent{ Value = SpaceTypeEnum.Tax });
        }
    }
}

public struct TaxSpaceTag : IComponentData
{ }

using Unity.Entities;
using UnityEngine;

public class TreasureSpaceAuthoring : MonoBehaviour
{
    [SerializeField] TreasureSpaceData data;
    class TreasureSpaceAuthoringBaker : Baker<TreasureSpaceAuthoring>
    {
        public override void Bake(TreasureSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.data.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.data.id });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.data.boardIndex });
            AddComponent(entity, new TreasureSpaceTag {});
            AddComponent(entity, new SpaceTypeComponent{ Value = SpaceTypeEnum.Treasure });
        }
    }
}

public struct TreasureSpaceTag : IComponentData
{ }

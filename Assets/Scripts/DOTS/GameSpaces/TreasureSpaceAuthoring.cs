using Unity.Entities;
using UnityEngine;

public class TreasureSpaceAuthoring : MonoBehaviour
{
    [SerializeField] string Name;

    class TreasureSpaceAuthoringBaker : Baker<TreasureSpaceAuthoring>
    {
        public override void Bake(TreasureSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = default });
            AddComponent(entity, new BoardIndexComponent { Value = default });
            AddComponent(entity, new TreasureSpaceTag {});
            AddComponent(entity, new SpaceTypeComponent{ Value = SpaceType.Treasure });
        }
    }
}

public struct TreasureSpaceTag : IComponentData
{ }

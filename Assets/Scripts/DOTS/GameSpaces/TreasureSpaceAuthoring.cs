using Unity.Entities;
using UnityEngine;

public class TreasureSpaceAuthoring : MonoBehaviour
{
    [SerializeField] int ID;
    [SerializeField] string Name;
    [SerializeField] int BoardIndex;

    class TreasureSpaceAuthoringBaker : Baker<TreasureSpaceAuthoring>
    {
        public override void Bake(TreasureSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.ID });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.BoardIndex });
            AddComponent(entity, new TreasureSpaceTag {});
        }
    }
}

public struct TreasureSpaceTag : IComponentData
{ }

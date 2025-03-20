using Unity.Entities;
using UnityEngine;

public class ChanceSpaceAuthoring : MonoBehaviour
{
	[SerializeField] ChancesSpaceData data;

    class ChanceSpaceAuthoringBaker : Baker<ChanceSpaceAuthoring>
    {
        public override void Bake(ChanceSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.data.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.data.id });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.data.boardIndex });
            AddComponent(entity, new ChanceSpaceTag { });
            AddComponent(entity, new SpaceTypeComponent{ Value = SpaceTypeEnum.Chance });
        }
    }
}

public struct ChanceSpaceTag : IComponentData
{ }

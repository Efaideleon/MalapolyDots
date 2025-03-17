using Unity.Entities;
using UnityEngine;

public class GoSpaceAuthoring : MonoBehaviour
{
	[SerializeField] GoSpaceData data;
	
    class GoSpaceAuthoringBaker : Baker<GoSpaceAuthoring>
    {
        public override void Bake(GoSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.data.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.data.id });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.data.boardIndex });
            AddComponent(entity, new GoSpaceTag { });
        }
    }
}

public struct GoSpaceTag : IComponentData
{ }

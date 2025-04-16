using Unity.Entities;
using UnityEngine;

public class GoSpaceAuthoring : MonoBehaviour
{
	[SerializeField] string Name;
	
    class GoSpaceAuthoringBaker : Baker<GoSpaceAuthoring>
    {
        public override void Bake(GoSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = default });
            AddComponent(entity, new BoardIndexComponent { Value = default });
            AddComponent(entity, new GoSpaceTag { });
            AddComponent(entity, new SpaceTypeComponent{ Value = SpaceType.Go });
        }
    }
}

public struct GoSpaceTag : IComponentData
{ }

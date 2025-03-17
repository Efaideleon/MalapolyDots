using Unity.Entities;
using UnityEngine;

public class GoToJailSpaceAuthoring : MonoBehaviour
{
    [SerializeField] GoToJailSpaceData data;

    class GoToJailSpaceAuthoringBaker : Baker<GoToJailSpaceAuthoring>
    {
        public override void Bake(GoToJailSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.data.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.data.id });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.data.boardIndex });
            AddComponent(entity, new GoToJailTag { });
        }
    }
}

public struct GoToJailTag : IComponentData
{ }

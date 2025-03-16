using Unity.Entities;
using UnityEngine;

public class GoToJailSpaceAuthoring : MonoBehaviour
{
    [SerializeField] int ID;
    [SerializeField] string Name;
    [SerializeField] int BoardIndex;

    class GoToJailSpaceAuthoringBaker : Baker<GoToJailSpaceAuthoring>
    {
        public override void Bake(GoToJailSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.ID });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.BoardIndex });
            AddComponent(entity, new GoToJailTag { });
        }
    }
}

public struct GoToJailTag : IComponentData
{ }

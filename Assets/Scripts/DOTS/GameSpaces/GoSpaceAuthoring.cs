using Unity.Entities;
using UnityEngine;

public class GoSpaceAuthoring : MonoBehaviour
{
    [SerializeField] int ID;
    [SerializeField] string Name;
    [SerializeField] int BoardIndex;

    class GoSpaceAuthoringBaker : Baker<GoSpaceAuthoring>
    {
        public override void Bake(GoSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.ID });
            AddComponent(entity, new SpaceIndexComponent { Value = authoring.BoardIndex });
        }
    }
}

using Unity.Entities;
using UnityEngine;

public class JailSpaceAuthoring : MonoBehaviour
{
    [SerializeField] int ID;
    [SerializeField] string Name;
    [SerializeField] int BoardIndex;

    class JailSpaceAuthoringBaker : Baker<JailSpaceAuthoring>
    {
        public override void Bake(JailSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.ID });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.BoardIndex });
            AddComponent(entity, new JailSpaceTag { });
        }
    }
}

public struct JailSpaceTag : IComponentData
{ }

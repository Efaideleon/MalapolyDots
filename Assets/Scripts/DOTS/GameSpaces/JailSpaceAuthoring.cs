using Unity.Entities;
using UnityEngine;

public class JailSpaceAuthoring : MonoBehaviour
{
    [SerializeField] JailSpaceData data;

    class JailSpaceAuthoringBaker : Baker<JailSpaceAuthoring>
    {
        public override void Bake(JailSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.data.Name });
            AddComponent(entity, new SpaceIDComponent { Value = authoring.data.id });
            AddComponent(entity, new BoardIndexComponent { Value = authoring.data.boardIndex });
            AddComponent(entity, new JailSpaceTag { });
        }
    }
}

public struct JailSpaceTag : IComponentData
{ }

using Unity.Entities;
using UnityEngine;

public class JailSpaceAuthoring : MonoBehaviour
{
    [SerializeField] string Name;

    class JailSpaceAuthoringBaker : Baker<JailSpaceAuthoring>
    {
        public override void Bake(JailSpaceAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new NameComponent { Value = authoring.Name });
            AddComponent(entity, new SpaceIDComponent { Value = default });
            AddComponent(entity, new BoardIndexComponent { Value = default });
            AddComponent(entity, new JailSpaceTag { });
            AddComponent(entity, new SpaceTypeComponent{ Value = SpaceType.Jail });
        }
    }
}

public struct JailSpaceTag : IComponentData
{ }

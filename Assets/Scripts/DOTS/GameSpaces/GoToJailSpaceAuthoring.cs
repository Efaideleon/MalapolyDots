using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class GoToJailSpaceAuthoring : MonoBehaviour
    {
        [SerializeField] string Name;

        class GoToJailSpaceAuthoringBaker : Baker<GoToJailSpaceAuthoring>
        {
            public override void Bake(GoToJailSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Name });
                AddComponent(entity, new SpaceIDComponent { Value = default });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new GoToJailTag { });
                AddComponent(entity, new SpaceTypeComponent{ Value = SpaceType.GoToJail });
            }
        }
    }

    public struct GoToJailTag : IComponentData
    { }

}

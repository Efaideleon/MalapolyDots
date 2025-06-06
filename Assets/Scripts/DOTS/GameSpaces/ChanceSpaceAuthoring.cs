using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class ChanceSpaceAuthoring : MonoBehaviour
    {
        [SerializeField] string Name;

        class ChanceSpaceAuthoringBaker : Baker<ChanceSpaceAuthoring>
        {
            public override void Bake(ChanceSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Name });
                AddComponent(entity, new SpaceIDComponent { Value = default });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new ChanceSpaceTag { });
                AddComponent(entity, new SpaceTypeComponent{ Value = SpaceType.Chance });
            }
        }
    }

    public struct ChanceSpaceTag : IComponentData
    { }
}

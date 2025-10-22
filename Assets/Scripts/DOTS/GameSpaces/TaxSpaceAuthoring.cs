using DOTS.DataComponents;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class TaxSpaceAuthoring : MonoBehaviour
    {
        public SpaceProperties Space;

        class TaxSpaceAuthoringBaker : Baker<TaxSpaceAuthoring>
        {
            public override void Bake(TaxSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Space.Name });
                AddComponent(entity, new SpaceIDComponent { Value = default });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new TaxSpaceTag { });
                AddComponent(entity, new SpaceTypeComponent{ Value = SpaceType.Tax });
            }
        }
    }

    public struct TaxSpaceTag : IComponentData
    { }
}

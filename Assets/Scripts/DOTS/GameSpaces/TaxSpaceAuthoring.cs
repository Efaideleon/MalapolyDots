using DOTS.DataComponents;
using DOTS.GameData.PlacesData;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class TaxSpaceAuthoring : MonoBehaviour
    {
        public TaxSpaceData Data;

        class TaxSpaceAuthoringBaker : Baker<TaxSpaceAuthoring>
        {
            public override void Bake(TaxSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Data.Name });
                AddComponent(entity, new SpaceIDComponent { Value = authoring.Data.id });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new TaxSpaceTag { });
                AddComponent(entity, new SpaceTypeComponent{ Value = authoring.Data.SpaceType });
            }
        }
    }

    public struct TaxSpaceTag : IComponentData
    { }
}

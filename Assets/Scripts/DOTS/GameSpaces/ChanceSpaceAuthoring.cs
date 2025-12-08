using DOTS.DataComponents;
using DOTS.GameData.PlacesData;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class ChanceSpaceAuthoring : MonoBehaviour
    {
        public ChancesSpaceData Data;

        class ChanceSpaceAuthoringBaker : Baker<ChanceSpaceAuthoring>
        {
            public override void Bake(ChanceSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NameComponent { Value = authoring.Data.Name });
                AddComponent(entity, new SpaceIDComponent { Value = authoring.Data.id });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new ChanceSpaceTag { });
                AddComponent(entity, new SpaceTypeComponent{ Value = authoring.Data.SpaceType });
                AddBuffer<ChanceActionDataBuffer>(entity);
            }
        }
    }

    public struct ChanceSpaceTag : IComponentData
    { }

    public struct ChanceActionDataBuffer : IBufferElementData
    {
        public int id;
        public FixedString64Bytes msg;
    }
}

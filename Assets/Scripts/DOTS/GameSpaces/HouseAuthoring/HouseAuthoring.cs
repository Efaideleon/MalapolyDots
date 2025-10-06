using Unity.Entities;
using UnityEngine;

public class HouseAuthoring : MonoBehaviour
{
    public class HouseBake : Baker<HouseAuthoring>
    {
        public override void Bake(HouseAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
            AddComponent(entity, new HouseColoring1 { Value = 0 });
            AddComponent(entity, new HouseColoring2 { Value = 0 });
            AddComponent(entity, new HouseColoring3 { Value = 0 });
            AddComponent(entity, new HouseColoring4 { Value = 0 });
            AddComponent(entity, new HouseClusterTag { });
        }
    }
}

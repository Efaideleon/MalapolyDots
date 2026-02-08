using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay
{
    public class CurrentActivePlayerGhostAuthoring : MonoBehaviour
    {
        public class CurrentActivePlayerGhostBaker : Baker<CurrentActivePlayerGhostAuthoring>
        {
            public override void Bake(CurrentActivePlayerGhostAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new CurrentActivePlayer { Entity = default });
            }
        }
    }

    [GhostComponent]
    public struct CurrentActivePlayer : IComponentData
    {
        [GhostField]
        public Entity Entity;
    }
}

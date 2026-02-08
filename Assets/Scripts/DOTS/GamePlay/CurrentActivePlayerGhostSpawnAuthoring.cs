using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay
{
    public class CurrentActivePlayerGhostSpawnAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public class CurrentActivePlayerGhostSpawnBaker : Baker<CurrentActivePlayerGhostSpawnAuthoring>
        {
            public override void Bake(CurrentActivePlayerGhostSpawnAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new CurrentActivePlayerGhostReference { Entity = GetEntity(authoring.Prefab, TransformUsageFlags.None) });
            }
        }
    }

    public struct CurrentActivePlayerGhostReference : IComponentData
    {
        public Entity Entity;
    }
}

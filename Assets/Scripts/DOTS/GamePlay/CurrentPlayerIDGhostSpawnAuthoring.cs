using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay
{
    public class CurrentPlayerIDGhostSpawnAuthoring : MonoBehaviour
    {
        public GameObject CurrentPlayerGhostPrefab;

        public class CurrentPlayerIDGhostBaker : Baker<CurrentPlayerIDGhostSpawnAuthoring>
        {
            public override void Bake(CurrentPlayerIDGhostSpawnAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new CurrentPlayerIDGhostReference { Entity = GetEntity(authoring.CurrentPlayerGhostPrefab, TransformUsageFlags.None) });
            }
        }
    }

    public struct CurrentPlayerIDGhostReference : IComponentData
    {
        public Entity Entity;
    }
}

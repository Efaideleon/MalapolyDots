using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay
{
    public class GameStateGhostSpawnAuthoring : MonoBehaviour
    {
        public GameObject Prefab;

        public class GameStateGhostSpawnBaker : Baker<GameStateGhostSpawnAuthoring>
        {
            public override void Bake(GameStateGhostSpawnAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new GameStateGhostReference { Entity = GetEntity(authoring.Prefab, TransformUsageFlags.None) });
            }
        }
    }

    public struct GameStateGhostReference : IComponentData
    {
        public Entity Entity;
    }
}

using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings
{
    public class SceneLoaderSpawnAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public class SceneLoaderSpawnBaker : Baker<SceneLoaderSpawnAuthoring>
        {
            public override void Bake(SceneLoaderSpawnAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new SceneLoaderGhostReference { Entity = GetEntity(authoring.Prefab, TransformUsageFlags.None) });
            }
        }
    }

    public struct SceneLoaderGhostReference : IComponentData
    {
        public Entity Entity;
    }
}

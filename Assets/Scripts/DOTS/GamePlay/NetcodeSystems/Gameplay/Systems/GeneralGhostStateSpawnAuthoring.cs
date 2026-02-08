using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    public class GeneralGhostStateSpawnAuthoring : MonoBehaviour
    {
        public GameObject Prefab;

        public class GeneralGhostStateSpawnAuthoringBaker : Baker<GeneralGhostStateSpawnAuthoring>
        {
            public override void Bake(GeneralGhostStateSpawnAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new GeneralGhostStateSpawnReference { Entity = GetEntity(authoring.Prefab, TransformUsageFlags.None) });
            }
        }
    }

    public struct GeneralGhostStateSpawnReference : IComponentData
    {
        public Entity Entity;
    }
}

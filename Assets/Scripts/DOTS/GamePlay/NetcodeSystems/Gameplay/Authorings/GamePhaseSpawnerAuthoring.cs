using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings
{
    public class GamePhaseSpawnerAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public class GamePhaseSpawnerBaker : Baker<GamePhaseSpawnerAuthoring>
        {
            public override void Bake(GamePhaseSpawnerAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new GamePhaseGhostReference { PrefabToSpawn = GetEntity(authoring.Prefab, TransformUsageFlags.None) });
            }
        }
    }

    public struct GamePhaseGhostReference : IComponentData
    {
        public Entity PrefabToSpawn;
    }
}

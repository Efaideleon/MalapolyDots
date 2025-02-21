using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    [SerializeField] public GameObject spawnPoint;

    public class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            float3 spawnPosition = authoring.spawnPoint.transform.position;
            AddComponent(entity, new SpawnPointComponent { Position = spawnPosition});
        }
    }
}

public struct SpawnPointComponent : IComponentData
{
    public float3 Position;
}

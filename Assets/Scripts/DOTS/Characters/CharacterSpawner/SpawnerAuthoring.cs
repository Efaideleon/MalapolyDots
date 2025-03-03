using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class SpawnerAuthoring : MonoBehaviour
{
    [SerializeField] public GameObject spawnPoint;
    [SerializeField] public GameObject[] wayPoints;

    public class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.None);
            float3 spawnPosition = authoring.spawnPoint.transform.position;
            AddComponent(entity, new SpawnPointComponent { Position = spawnPosition });

            var wayPointsBuffer = AddBuffer<WayPointBufferElement>(entity);
            for (int i = 0; i < authoring.wayPoints.Length; i++)
            {
                wayPointsBuffer.Add( new WayPointBufferElement{ WayPoint =  authoring.wayPoints.Length });
            }
        }
    }
}

public struct SpawnPointComponent : IComponentData
{
    public float3 Position;
}

public struct WayPointBufferElement : IBufferElementData
{
    public float WayPoint;
}

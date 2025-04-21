using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.Characters.CharacterSpawner
{
    public class RouteAuthoring : MonoBehaviour
    {
        [SerializeField] public GameObject spawnPoint;
        [SerializeField] public GameObject[] wayPoints;

        public class RouteBaker : Baker<RouteAuthoring>
        {
            public override void Bake(RouteAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                float3 spawnPosition = authoring.spawnPoint.transform.position;
                AddComponent(entity, new SpawnPointComponent { Position = spawnPosition });

                var wayPointsBuffer = AddBuffer<WayPointBufferElement>(entity);
                for (int i = 0; i < authoring.wayPoints.Length; i++)
                {
                    wayPointsBuffer.Add( new WayPointBufferElement{ WayPoint =  authoring.wayPoints[i].transform.position });
                }

                AddComponent(entity, new WayPointsTag{});
            }
        }
    }

    public struct SpawnPointComponent : IComponentData
    {
        public float3 Position;
    }

    public struct WayPointBufferElement : IBufferElementData
    {
        public float3 WayPoint;
    }

    public struct WayPointsTag : IComponentData
    {}
}

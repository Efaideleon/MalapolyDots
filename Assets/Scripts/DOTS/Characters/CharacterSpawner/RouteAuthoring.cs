using DOTS.DataComponents;
using Unity.Collections;
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
                    authoring.wayPoints[i].TryGetComponent<SpaceWayPoint>(out var spaceWayPoint);
                    var name = spaceWayPoint == null ? "None" : spaceWayPoint.space.Name;

                    wayPointsBuffer.Add( new WayPointBufferElement 
                    { 
                        WayPoint =  authoring.wayPoints[i].transform.position,
                        Name = name
                    });
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
        public FixedString64Bytes Name;
    }

    public struct WayPointsTag : IComponentData
    {}
}

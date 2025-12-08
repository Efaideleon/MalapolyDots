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
                AddComponent(entity, new WayPointsTag { });

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<WaypointsBlobAsset>();

                var positions = builder.Allocate(ref root.Waypoints, authoring.wayPoints.Length);

                for (int i = 0; i < authoring.wayPoints.Length; i++)
                {
                    authoring.wayPoints[i].TryGetComponent<SpaceWayPoint>(out var spaceWayPoint);
                    var isLandingSpot = spaceWayPoint != null;
                    positions[i] = new Waypoint 
                    {
                        Position = authoring.wayPoints[i].transform.position,
                        IsLandingSpot = isLandingSpot
                    };
                }

                var blobRef = builder.CreateBlobAssetReference<WaypointsBlobAsset>(Allocator.Persistent);
                var wayPointsBlobRef = new WaypointsBlobRef { Reference = blobRef };
                AddBlobAsset(ref blobRef, out _);
                AddComponent(entity, wayPointsBlobRef);
            }
        }
    }

    public struct WaypointsBlobRef : IComponentData
    {
        public BlobAssetReference<WaypointsBlobAsset> Reference;
    }

    public struct Waypoint : IComponentData
    {
        public float3 Position;
        public bool IsLandingSpot;
    }

    public struct WaypointsBlobAsset
    {
        public BlobArray<Waypoint> Waypoints;
    }

    public struct SpawnPointComponent : IComponentData
    {
        public float3 Position;
    }

    public struct WayPointBufferElement : IBufferElementData
    {
        public float3 WayPoint;
        public int BoardIndex;
    }

    public struct WayPointsTag : IComponentData
    { }
}

using Assets.Scripts.DOTS.DataComponents;
using DOTS.DataComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.NetCode;

namespace DOTS.GameSpaces
{
    // TODO:
    // 1. Change this to run in the server.
    // 2. The buildings should spawn as ghosts.
    //
    [BurstCompile]
    public partial struct PlacesSpawnJob : IJobParallelFor
    {
        [ReadOnly]
        public DynamicBuffer<PlacesPrefabBuffer> prefabs;
        public EntityCommandBuffer.ParallelWriter ecbParallel;

        public void Execute(int index)
        {
            var entity = ecbParallel.Instantiate(index, prefabs[index].entity);
            ecbParallel.SetComponent(index, entity, new BoardIndexComponent { Value = prefabs[index].BoardIndex });
        }
    }

    // TODO: we need to spawn this building before the client connects?
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct PlacesSpawner : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlacesPrefabBuffer>();
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<CurrentScene>();
            state.RequireForUpdate<SceneLoader>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Run when the gamescene is loaded.
            if (SystemAPI.GetSingleton<CurrentScene>().sceneGUID == SystemAPI.GetSingleton<SceneLoader>().GameSceneGuid)
            {
                var placesPrefabs = SystemAPI.GetSingletonBuffer<PlacesPrefabBuffer>();
                state.EntityManager.CreateSingleton(new IndexToBoardHashMap { Map = new(placesPrefabs.Length, Allocator.Persistent) });

                UnityEngine.Debug.Log($"[PlacesSpawner] | spawning places..");
                var job = new PlacesSpawnJob
                {
                    prefabs = placesPrefabs,
                    ecbParallel = GetECB(ref state).AsParallelWriter(),
                };
                var jobHandle = job.Schedule(placesPrefabs.Length, 2);
                jobHandle.Complete();

                state.Enabled = false;
            }
        }

        public void OnDestroy(ref SystemState state)
        {
            if (SystemAPI.TryGetSingletonRW<IndexToBoardHashMap>(out var singleton))
            {
                if (singleton.ValueRW.Map.IsCreated)
                {
                    singleton.ValueRW.Map.Dispose();
                }
            }
        }

        [BurstCompile]
        public readonly EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecb.CreateCommandBuffer(state.WorldUnmanaged);
        }
    }

    public struct PlacesSpawnedTag : IComponentData { }

    public struct IndexToBoardHashMap : IComponentData
    {
        public NativeParallelHashMap<int, Entity> Map;
    }
}

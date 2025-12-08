using DOTS.DataComponents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DOTS.GameSpaces
{
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

    [BurstCompile]
    public partial struct PlacesSpawner : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlacesPrefabBuffer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var placesPrefabs = SystemAPI.GetSingletonBuffer<PlacesPrefabBuffer>();
            state.EntityManager.CreateSingleton(new IndexToBoardHashMap { Map = new(placesPrefabs.Length, Allocator.Persistent) });

            var job = new PlacesSpawnJob
            {
                prefabs = placesPrefabs,
                ecbParallel = GetECB(ref state).AsParallelWriter(),
            };
            var jobHandle = job.Schedule(placesPrefabs.Length, 2);
            state.Dependency = jobHandle;

            state.Enabled = false;
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

    public struct IndexToBoardHashMap : IComponentData
    {
        public NativeParallelHashMap<int, Entity> Map;
    }
}

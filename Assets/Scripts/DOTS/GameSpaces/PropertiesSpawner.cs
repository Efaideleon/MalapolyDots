using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace DOTS.GameSpaces
{
    [BurstCompile]
    public partial struct PropertySpawnJob : IJobParallelFor
    {
        [ReadOnly]
        public DynamicBuffer<PropertiesPrefabBuffer> prefabs;
        public EntityCommandBuffer.ParallelWriter ecbParallel;

        public void Execute(int index)
        {
            ecbParallel.Instantiate(index, prefabs[index].entity);
        }
    }

    [BurstCompile]
    public partial struct PropertiesSpawner : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PropertiesPrefabBuffer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var propertiesBuffer = SystemAPI.GetSingletonBuffer<PropertiesPrefabBuffer>();

            var job = new PropertySpawnJob
            {
                prefabs = propertiesBuffer,
                ecbParallel = GetECB(ref state).AsParallelWriter()
            };
            var jobHandle = job.Schedule(propertiesBuffer.Length, 2);
            state.Dependency = jobHandle;

            state.Enabled = false;
        }

        [BurstCompile]
        public readonly EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecb.CreateCommandBuffer(state.WorldUnmanaged);
        }
    }
}

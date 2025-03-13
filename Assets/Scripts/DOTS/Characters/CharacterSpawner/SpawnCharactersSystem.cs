using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using System.Linq;

public struct SpawnFlag : IComponentData
{
    public bool Value;
}

[BurstCompile]
public partial struct SpawnParallelJob : IJobParallelFor
{
    [ReadOnly]
    public DynamicBuffer<CharacterSelectedBuffer> charactersSelected;
    [ReadOnly]
    public NativeParallelHashMap<FixedString64Bytes, Entity> prefabLookUp;
    [ReadOnly]
    public NativeArray<float3> Positions;
    public EntityCommandBuffer.ParallelWriter ecbParallel;

    [BurstCompile]
    public void Execute(int index)
    {
        if (prefabLookUp.TryGetValue(charactersSelected[index].Value, out var prefab))
        {
            InstantiatePrefab(ecbParallel, prefab, Positions[index], index);
        }
    }

    [BurstCompile]
    private void InstantiatePrefab(EntityCommandBuffer.ParallelWriter ecbParallel, Entity prefab, float3 position, int sort_key)
    {
        var instance = ecbParallel.Instantiate(sort_key, prefab);
        ecbParallel.SetComponent(sort_key, instance, new LocalTransform
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = 1f
        });

        ecbParallel.SetComponent(sort_key, instance, new PlayerID { Value = sort_key });
    }
}

[BurstCompile]
public partial struct SpawnCharactersSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameDataComponent>();
        state.RequireForUpdate<CharactersBufferTag>();
        state.RequireForUpdate<WayPointsTag>();

        var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<SpawnFlag>(),
        });

        SystemAPI.SetComponent(entity, new SpawnFlag { Value = false });
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var characterSelectedBuffer = SystemAPI.GetSingletonBuffer<CharacterSelectedBuffer>();
        var charactersPrefabBuffer = SystemAPI.GetSingletonBuffer<CharacterEntityBuffer>();
        var spawnPosition = SystemAPI.GetSingleton<SpawnPointComponent>().Position;
        var positions = new NativeArray<float3>(characterSelectedBuffer.Length, Allocator.TempJob);
        var spawnRadius = 4;
        CalculatePositions(positions, spawnPosition, spawnRadius);

        var prefabLookUp = new NativeParallelHashMap<FixedString64Bytes, Entity>(charactersPrefabBuffer.Length, Allocator.TempJob);
        for (int i = 0; i < charactersPrefabBuffer.Length; i++)
        {
            var prefabName = SystemAPI.GetComponent<NameDataComponent>(charactersPrefabBuffer[i].Prefab).Value;
            prefabLookUp.Add(prefabName, charactersPrefabBuffer[i].Prefab);
        }

        var job = new SpawnParallelJob
        {
            charactersSelected = characterSelectedBuffer,
            prefabLookUp = prefabLookUp,
            Positions = positions,
            ecbParallel = GetEntityCommandBuffer(ref state).AsParallelWriter(),
        };

        var jobHandle = job.Schedule(characterSelectedBuffer.Length, 5);
        jobHandle = positions.Dispose(jobHandle);
        jobHandle = prefabLookUp.Dispose(jobHandle);

        state.Dependency = jobHandle;
        foreach (var spawnFlag in SystemAPI.Query<RefRW<SpawnFlag>>())
        {
            spawnFlag.ValueRW.Value = true;
        }

        state.Enabled = false;
    }

    [BurstCompile]
    private readonly EntityCommandBuffer GetEntityCommandBuffer(ref SystemState state)
    {
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
        return ecb.CreateCommandBuffer(state.WorldUnmanaged);
    }

    [BurstCompile]
    private readonly void CalculatePositions(NativeArray<float3> positions, float3 spawnPosition, float radius)
    {
        float angleStep = math.PI * 2f / positions.Length;

        for (int i = 0; i < positions.Length; i++)
        {
            float angle = i * angleStep;
            // following cartesian plane
            float x = math.cos(angle) * radius;
            float y = math.sin(angle) * radius;

            // following unity plane
            positions[i] = new float3(x, 0f, y) + spawnPosition;
        }
    }
}

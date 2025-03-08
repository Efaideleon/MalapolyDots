using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

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
    public DynamicBuffer<CharacterEntityBuffer> characterPrefabs;
    [ReadOnly]
    public NativeArray<FixedString64Bytes> allCharacterNames;
    [ReadOnly]
    public NativeArray<float3> Positions;
    public EntityCommandBuffer.ParallelWriter ecbParallel;

    [BurstCompile]
    public void Execute(int index)
    {
        for (int i = 0; i < characterPrefabs.Length; i++)
        {
            if (allCharacterNames[i] == charactersSelected[index].Value)
            {
                InstantiatePrefab(ecbParallel, characterPrefabs[i].Prefab, Positions[i], index);
            }
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

        // The first player in the list has the first turn.
        if (sort_key == 0)
        {
            ecbParallel.SetComponent(sort_key, instance, new TurnComponent { IsActive = true });
        }
    }
}

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
        state.Enabled = false;

        var characterSelectedBuffer = SystemAPI.GetSingletonBuffer<CharacterSelectedBuffer>();
        var charactersPrefabBuffer = SystemAPI.GetSingletonBuffer<CharacterEntityBuffer>();
        var spawnPosition = SystemAPI.GetSingleton<SpawnPointComponent>().Position;
        var positions = new NativeArray<float3>(characterSelectedBuffer.Length, Allocator.TempJob);
        CalculatePositions(positions, spawnPosition, 4);

        var characterNames = new NativeArray<FixedString64Bytes>(charactersPrefabBuffer.Length, Allocator.TempJob);

        for (int i = 0; i < charactersPrefabBuffer.Length; i++)
        {
            characterNames[i] = SystemAPI.GetComponent<NameDataComponent>(charactersPrefabBuffer[i].Prefab).Value;
        }

        var job = new SpawnParallelJob
        {
            charactersSelected = characterSelectedBuffer,
            characterPrefabs = charactersPrefabBuffer,
            allCharacterNames = characterNames,
            Positions = positions,
            ecbParallel = GetEntityCommandBuffer(ref state).AsParallelWriter(),
        };
        var spawnJobHandle = job.Schedule(characterSelectedBuffer.Length, 1);
        spawnJobHandle.Complete();
        state.Dependency.Complete();
        positions.Dispose();
        characterNames.Dispose();
        foreach (var spawnFlag in SystemAPI.Query<RefRW<SpawnFlag>>())
        {
            spawnFlag.ValueRW.Value = true;
        }
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
        for (int i = 0; i < positions.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / positions.Length;

            // following cartesian plane
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            // following unity plane
            positions[i] = new float3(x, 0f, y) + spawnPosition;
        }
    }
}

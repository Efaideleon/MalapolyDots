using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;
using Unity.Transforms;
using Unity.Mathematics;

public partial struct SpawnCharactersSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameDataComponent>();
        state.RequireForUpdate<CharactersBufferTag>();
        state.RequireForUpdate<WayPointsTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        var characterSelectedBuffer = SystemAPI.GetSingletonBuffer<CharacterSelectedBuffer>();
        var charactersPrefabBuffer = SystemAPI.GetSingletonBuffer<CharacterEntityBuffer>();
        var spawnPosition = SystemAPI.GetSingleton<SpawnPointComponent>().Position;
        var positions = new NativeArray<float3>(characterSelectedBuffer.Length, Allocator.Temp);
        CalculatePositions(positions, spawnPosition, 4);

        for (int i = 0; i < characterSelectedBuffer.Length; i++)
        {
            var characterSelectedName = characterSelectedBuffer[i].Value;
            foreach (var character in charactersPrefabBuffer)
            {
                var prefabName = SystemAPI.GetComponent<NameDataComponent>(character.Prefab).Value;
                if (characterSelectedName == prefabName)
                {
                    InstantiatePrefab(ecb, character.Prefab, positions[i], i);
                }
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private void InstantiatePrefab(EntityCommandBuffer ecb, Entity prefab, float3 position, int i)
    {
        var instance = ecb.Instantiate(prefab);
        ecb.SetComponent(instance, new LocalTransform
        {
            Position = position,
            Rotation = quaternion.identity,
            Scale = 1f
        });

        // The first player in the list has the first turn.
        if (i == 0)
        {
            ecb.SetComponent(prefab, new TurnComponent { IsActive = true });
        }
    }

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

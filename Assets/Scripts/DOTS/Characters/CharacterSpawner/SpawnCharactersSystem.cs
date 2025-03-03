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
        state.RequireForUpdate<PrefabReferenceComponent>();
        state.RequireForUpdate<WayPointsTag>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        Debug.Log("Spawning...");
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        float3 spawnPosition = SystemAPI.GetSingleton<SpawnPointComponent>().Position;

        // Entity for the Scriptable Object.
        foreach (var (gameDataComponent, entityDataSO) in SystemAPI.Query<RefRO<GameDataComponent>>().WithEntityAccess())
        {
            var charactersbuffer = SystemAPI.GetBuffer<CharacterSelectedBuffer>(entityDataSO);
            NativeArray<float3> positions = new(charactersbuffer.Length, Allocator.Temp);
            CalculatePositions(positions, spawnPosition, 4);

            for (int i = 0; i < charactersbuffer.Length; i++)
            {
                var characterNameElement = charactersbuffer[i];
                // Getting components for character entity
                foreach (var (prefabReference, NameComponent, turnComponent, characterEntity) in
                        SystemAPI.Query<RefRW<PrefabReferenceComponent>, RefRW<NameDataComponent>, RefRW<TurnComponent>>().WithEntityAccess())
                {
                    if (NameComponent.ValueRO.Name == characterNameElement.Value)
                    {
                        var instance = ecb.Instantiate(prefabReference.ValueRW.Value);
                        ecb.SetComponent(instance, new LocalTransform
                        {
                            Position = positions[i],
                            Rotation = quaternion.identity,
                            Scale = 1f
                        });

                        // The first player in the list has the first turn.
                        if (i == 0)
                        {
                            ecb.SetComponent(characterEntity, new TurnComponent { IsActive = true });
                        }

                    }
                }
            }
        }
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
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

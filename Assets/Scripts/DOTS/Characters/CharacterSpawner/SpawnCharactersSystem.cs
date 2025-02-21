using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using DOTS;
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
        Debug.Log("Testing");
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
        float3 spawnPosition = SystemAPI.GetSingleton<SpawnPointComponent>().Position;

        foreach (var (gameDataComponent, entity) in SystemAPI.Query<RefRO<GameDataComponent>>().WithEntityAccess())
        {
            var charactersbuffer = SystemAPI.GetBuffer<CharacterSelectedBuffer>(entity);
            NativeArray<float3> positions = new(charactersbuffer.Length, Allocator.Temp);
            CalculatePositions(positions, spawnPosition, 4);

            for (int i = 0; i < charactersbuffer.Length; i++)
            {
                var characterNameElement = charactersbuffer[i];
                foreach (var (prefabReference, NameComponent) in
                        SystemAPI.Query<RefRW<PrefabReferenceComponent>, RefRW<NameDataComponent>>())
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

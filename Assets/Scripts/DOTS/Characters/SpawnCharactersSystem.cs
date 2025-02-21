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
        float3 spawnPosition = new (0f, 0f, 0f);
        foreach (var (gameDataComponent, entity) in SystemAPI.Query<RefRO<GameDataComponent>>().WithEntityAccess())
        {
            var charactersbuffer = SystemAPI.GetBuffer<CharacterSelectedBuffer>(entity);
            foreach (var characterNameElement in charactersbuffer)
            {
                foreach (var (prefabReference, NameComponent) in
                        SystemAPI.Query<RefRW<PrefabReferenceComponent>, RefRW<NameDataComponent>>())
                {
                    if (NameComponent.ValueRO.Name == characterNameElement.Value)
                    {
                        var instance = ecb.Instantiate(prefabReference.ValueRW.Value);
                        ecb.SetComponent(instance, new LocalTransform
                        {
                            Position = spawnPosition,
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

    [BurstDiscard]
    static void DebugLog(string message)
    {
        Debug.Log(message);
    }
}

using Unity.Entities;
using UnityEngine;
using DOTS;
using Unity.Burst;

public partial struct SpawnCharactersSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameDataComponent>();
        Debug.Log("Testing");
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        foreach (var (gameDataComponent, entity) in SystemAPI.Query<RefRO<GameDataComponent>>().WithEntityAccess())
        {
            var charactersbuffer = SystemAPI.GetBuffer<CharacterSelectedBuffer>(entity);
            foreach (var characterNameElement in charactersbuffer)
            {
                Debug.Log($"character: {characterNameElement.Value}");
                foreach (var (prefabTag, NameComponent, prefabEntity) in SystemAPI.Query<RefRO<PrefabTag>, RefRO<NameDataComponent>>().WithEntityAccess())
                {
                    if (NameComponent.ValueRO.Name == characterNameElement.Value)
                    {
                        Debug.Log($"Instantiating prefab: {NameComponent.ValueRO.Name}");
                        state.EntityManager.Instantiate(prefabEntity);
                    }
                }
            }
            Debug.Log($"{gameDataComponent.ValueRO.NumberOfPlayers}");
        }
    }

    [BurstDiscard]
    static void DebugLog(string message)
    {
        Debug.Log(message);
    }
}

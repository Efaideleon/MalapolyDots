using Unity.Entities;
using UnityEngine;
using DOTS;
using Unity.Collections;
using Unity.Burst;
using System.Linq;

public partial struct SpawnCharactersSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<GameDataBlobComponent>();
        //state.RequireForUpdate<PrefabComponent>();
        //state.RequireForUpdate<NameDataComponent>();
        Debug.Log("Testing");
    }

    public void OnUpdate(ref SystemState state)
    {
        UnityEngine.Debug.Log("Something");
        state.Enabled = false;
        EntityCommandBuffer entityCommandBuffer = new(Allocator.Temp);
        var gameDataComponent = SystemAPI.GetSingleton<GameDataComponent>();
        for (int i = 0; i < gameDataComponent.CharactersSelected.Length; i++)
        {
            var charSelectedNameFixed = gameDataComponent.CharactersSelected[i];

            foreach (var (prefabComp, nameComp) in SystemAPI.Query<RefRO<PrefabComponent>, RefRO<NameDataComponent>>())
            {
                if (nameComp.ValueRO.Name == charSelectedNameFixed)
                {
                    entityCommandBuffer.Instantiate(prefabComp.ValueRO.prefab);
                }
            }
        }
        entityCommandBuffer.Playback(state.EntityManager);
    }

    [BurstDiscard]
    static void DebugLog(string message)
    {
        Debug.Log(message);
    }
}

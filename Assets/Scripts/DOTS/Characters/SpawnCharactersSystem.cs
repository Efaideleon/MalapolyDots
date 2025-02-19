using Unity.Entities;
using DOTS;
using Unity.Collections;
using UnityEngine.Timeline;

public partial struct SpawnCharactersSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CharactersComponent>();
        state.RequireForUpdate<GameDataBlobComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        // Get all the entities with a prefab component
        NativeArray<Entity> characterEntities = SystemAPI.Query<RefRO<PrefabComponent>>().WithEntityAccess().ToEntityArray(Allocator.Temp);
        foreach (var gameDataComponent in SystemAPI.Query<RefRO<GameDataBlobComponent>>())
        {
            ref var gameDataBlob = ref gameDataComponent.ValueRO.gameDataBlob.Value;

            for (int i = 0; i < gameDataBlob.CharactersSelected.Length; i++)
            {
                ref BlobString characterSelectedName = ref gameDataBlob.CharactersSelected[i];
                FixedString64Bytes charSelectedNameFixed = default;
                characterSelectedName.CopyTo(ref charSelectedNameFixed);

                foreach (var characterEntity in characterEntities)
                {
                    // Get the name component from the entity
                    if (characterEntity.ValueRO.Name == charSelectedNameFixed)
                    {

                    }
                }
            }

        }

        state.EntityManager.Instantiate(charactersComponent.avocado);
    }
}

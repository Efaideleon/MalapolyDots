using Unity.Entities;
using DOTS;

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
        var charactersComponent = SystemAPI.GetSingleton<CharactersComponent>();
        foreach (var gameDataComponent in SystemAPI.Query<RefRO<GameDataBlobComponent>>())
        {
            ref var gameDataBlob = ref gameDataComponent.ValueRO.gameDataBlob.Value;

            for (int i = 0; i < gameDataBlob.CharactersSelected.Length; i++)
            {
                ref BlobString characterName = ref gameDataBlob.CharactersSelected[i];
            }
        }

        state.EntityManager.Instantiate(charactersComponent.avocado);
    }
}

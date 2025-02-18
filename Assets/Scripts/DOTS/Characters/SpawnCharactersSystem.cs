using Unity.Entities;
using DOTS;

public partial struct SpawnCharactersSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CharactersComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var charactersComponent = SystemAPI.GetSingleton<CharactersComponent>();
        foreach (var (gameDataBlobComponent, entity) in SystemAPI.Query<GameDataBlobComponent>().WithEntityAccess())
        {
            ref var gameDataBlob = ref gameDataBlobComponent.gameDataBlob.Value;
            int numOfPlayers = gameDataBlob.Values[0];
        }

        state.EntityManager.Instantiate(charactersComponent.avocado);
    }
}

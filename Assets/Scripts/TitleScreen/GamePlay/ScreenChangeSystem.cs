using Unity.Entities;

public enum ScreenType
{
    Title,
    NumOfPlayers,
    NumOfRounds,
    CharacterSelection
}

public struct ChangeScreenEventBuffer : IBufferElementData
{
    public ScreenType ScreenType;
}

public partial struct ScreenChangeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingletonBuffer<ChangeScreenEventBuffer>();
        state.RequireForUpdate<ChangeScreenEventBuffer>();
        state.RequireForUpdate<TitleScreenControllers>();
        state.RequireForUpdate<LoginData>();
        state.RequireForUpdate<CurrentPlayerNumberPickingCharacter>();
    }

    public void OnUpdate(ref SystemState state)
    {
        // TODO: Based on the screentype we can hide and show screens
        foreach (var eventBuffer in
                SystemAPI.Query<
                    DynamicBuffer<ChangeScreenEventBuffer>
                >()
                .WithChangeFilter<ChangeScreenEventBuffer>())
        {
            var titleScreenControllers = SystemAPI.ManagedAPI.GetSingleton<TitleScreenControllers>();
            if (titleScreenControllers == null)
                break;
            if (titleScreenControllers.TitleScreenController == null ||
                    titleScreenControllers.CharacterSelectionControler == null ||
                    titleScreenControllers.NumOfRoundsController == null ||
                    titleScreenControllers.NumOfPlayersController == null
                )
                break;

            foreach (var e in eventBuffer)
            {
                switch (e.ScreenType)
                {
                    case ScreenType.Title:
                        titleScreenControllers.TitleScreenController.HideScreen();
                        titleScreenControllers.NumOfPlayersController.ShowScreen();
                        break;
                    case ScreenType.NumOfPlayers:
                        var numberOfPlayers = SystemAPI.GetSingleton<LoginData>().NumberOfPlayers;
                        if (numberOfPlayers >= 0)
                        {
                            titleScreenControllers.NumOfPlayersController.HideScreen();
                            titleScreenControllers.CharacterSelectionControler.ShowScreen();
                        }
                        break;
                    case ScreenType.CharacterSelection:
                        if (SystemAPI.GetSingleton<IsCharacterAvailable>().Value)
                        {
                            var charactersSelected = SystemAPI.GetSingletonBuffer<CharacterSelectedNameBuffer>();
                            var nameToAdd = SystemAPI.GetSingleton<IsCharacterAvailable>().Name;
                            charactersSelected.Add(new CharacterSelectedNameBuffer { Name = nameToAdd });
                            var currentPlayer = SystemAPI.GetSingletonRW<CurrentPlayerNumberPickingCharacter>();
                            var numOfPlayers = SystemAPI.GetSingleton<LoginData>().NumberOfPlayers;
                            if (currentPlayer.ValueRO.Value < numOfPlayers)
                            {
                                currentPlayer.ValueRW.Value += 1;
                                titleScreenControllers.CharacterSelectionControler.Context = new CharacterSelectionContext 
                                { 
                                    PlayerNumber = currentPlayer.ValueRO.Value 
                                };
                                titleScreenControllers.CharacterSelectionControler.Update();
                            }
                            else
                            {
                                titleScreenControllers.CharacterSelectionControler.HideScreen();
                                titleScreenControllers.NumOfRoundsController.ShowScreen();
                            }
                        }
                        break;
                    case ScreenType.NumOfRounds:
                        titleScreenControllers.NumOfRoundsController.HideScreen();
                        UnityEngine.Debug.Log("Change Scene");
                        break;
                }
            }
            eventBuffer.Clear();
        }
    }
}

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
        state.RequireForUpdate<NumOfPlayerPicking>();
        state.RequireForUpdate<ConfirmButtonEventBuffer>();
        state.RequireForUpdate<LastCharacterClicked>();
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
                            // TODO: Maybe make all of this into components for entities?
                            titleScreenControllers.CharacterSelectionControler.Context = new CharacterSelectionContext
                            {
                                PlayerNumber = SystemAPI.GetSingleton<NumOfPlayerPicking>().Value
                            };
                            titleScreenControllers.CharacterSelectionControler.Update();
                            titleScreenControllers.CharacterSelectionControler.ShowScreen();
                        }
                        break;
                    case ScreenType.CharacterSelection:
                        var characterSelected = SystemAPI.GetSingleton<LastCharacterClicked>().Value;
                        if (characterSelected.State == AvailableState.Available)
                        {
                            var currPlayerNum = SystemAPI.GetSingletonRW<NumOfPlayerPicking>();
                            var totalNumOfPlayers = SystemAPI.GetSingleton<LoginData>().NumberOfPlayers;
                            if (currPlayerNum.ValueRO.Value < totalNumOfPlayers + 1)
                            {
                                currPlayerNum.ValueRW.Value += 1;
                                SystemAPI.GetSingletonBuffer<ConfirmButtonEventBuffer>()
                                    .Add(new ConfirmButtonEventBuffer{ character = characterSelected.Type });
                            }
                            if (currPlayerNum.ValueRW.Value == totalNumOfPlayers + 1)
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

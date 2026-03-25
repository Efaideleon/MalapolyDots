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

            foreach (var ScreenChangeRequest in eventBuffer)
            {
                switch (ScreenChangeRequest.ScreenType)
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
                        // Last character clicked must be cleared after it gets confirmed.
                        ref var characterSelected = ref SystemAPI.GetSingletonRW<LastCharacterClicked>().ValueRW;

                        // If the character is not available, do nothing.
                        if (characterSelected.Value.State != AvailableState.Available)
                            break;

                        // Assume NumOfPlayerPicking is initialized to 1.
                        var currPlayerNum = SystemAPI.GetSingletonRW<NumOfPlayerPicking>();
                        var totalNumOfPlayers = SystemAPI.GetSingleton<LoginData>().NumberOfPlayers;

                        bool isLastPlayerPicking = currPlayerNum.ValueRO.Value >= totalNumOfPlayers;

                        // Confirm the character selection.
                        currPlayerNum.ValueRW.Value += 1;
                        SystemAPI.GetSingletonBuffer<ConfirmButtonEventBuffer>()
                            .Add(new ConfirmButtonEventBuffer { character = characterSelected.Value.Type });

                        // Clear the LastCharacterClicked
                        characterSelected.Value.Type = Character.None;
                        characterSelected.Value.State = AvailableState.Unavailable;

                        // If it's the last player picking, go the next screen.
                        if (isLastPlayerPicking)
                        {
                            titleScreenControllers.CharacterSelectionControler.HideScreen();
                            titleScreenControllers.NumOfRoundsController.ShowScreen();
                        }
                        break;
                    case ScreenType.NumOfRounds:
                        var numOfRoundsClicked = SystemAPI.GetSingleton<LastNumberOfRoundsClicked>().Value;
                        if (numOfRoundsClicked > 0)
                        {
                            SystemAPI.GetSingletonBuffer<NumberOfRoundsConfirmEventBuffer>()
                                .Add(new NumberOfRoundsConfirmEventBuffer { });
                            titleScreenControllers.NumOfRoundsController.HideScreen();
                            SystemAPI.GetSingletonBuffer<SceneChangeEventBuffer>()
                                .Add(new SceneChangeEventBuffer { SceneID = SceneID.Game });
                        }
                        break;
                }
            }
            eventBuffer.Clear();
        }
    }
}

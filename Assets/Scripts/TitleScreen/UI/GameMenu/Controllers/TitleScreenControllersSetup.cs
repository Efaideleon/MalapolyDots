using System;
using UI.GameMenu;
using Unity.Entities;
using UnityEngine.UIElements;

public class TitleScreenControllers : IComponentData
{
    public TitleScreenController TitleScreenController;
    public NumOfRoundsController NumOfRoundsController;
    public NumOfPlayersController NumOfPlayersController;
    public CharacterSelectionController CharacterSelectionControler;
}

public partial struct TitleScreenControllersSetup : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        state.EntityManager.CreateSingleton(new TitleScreenControllers
        {
            TitleScreenController = null,
            CharacterSelectionControler = null,
            NumOfPlayersController = null,
            NumOfRoundsController = null
        });

        state.RequireForUpdate<TitleScreenControllers>();
        state.RequireForUpdate<TitleScreenCanvasReference>();
#if UNITY_EDITOR
        state.RequireForUpdate<LoginData>();
#endif
    }

    public void OnStartRunning(ref SystemState state)
    {
        var titleScreenCanvasRef = SystemAPI.ManagedAPI.GetSingleton<TitleScreenCanvasReference>();
        var titleScreenCanvasGO = titleScreenCanvasRef.uiDocumentGO;
        if (titleScreenCanvasGO == null)
            return;

        var uiGameObject = UnityEngine.Object.Instantiate(titleScreenCanvasRef.uiDocumentGO);
        var uiDocument = uiGameObject.GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            UnityEngine.Object.Destroy(uiGameObject);
            return;
        }

        var titleScreenRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-menu-root");
        TitleScreen titleScreen = new(titleScreenRoot);
        NumOfRoundsScreen numOfRoundsScreen = new(titleScreenRoot);
        NumberOfPlayersScreen numberOfPlayersScreen = new(titleScreenRoot);
        CharacterSelectionScreen characterSelectionScreen = new(titleScreenRoot);

        var changeScreenEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<ChangeScreenEventBuffer>().Build();
        var roundsDataEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<NumberOfRoundsEventBuffer>().Build();
        var playersDataEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<NumberOfPlayersEventBuffer>().Build();

#if UNITY_EDITOR
        var debugStartButton = titleScreenRoot.Q<Button>("debug-start-button") ?? throw new ArgumentNullException("[TitleScreenControllersSetup] no \"debug-start-button\"");
        var loginData = SystemAPI.GetSingletonRW<LoginData>();
        var characterSelectedNameBuffer = SystemAPI.GetSingletonBuffer<CharacterSelectedNameBuffer>();
        var lastNumOfRoundsClickedQuery = SystemAPI.QueryBuilder().WithAllRW<LastNumberOfRoundsClicked>().Build();
        debugStartButton.clickable.clicked += () =>
        {
            var lastNumOfRoundsClicked = lastNumOfRoundsClickedQuery.GetSingletonRW<LastNumberOfRoundsClicked>();
            SetDataForGameToStart(
                    ref loginData.ValueRW,
                    ref characterSelectedNameBuffer,
                    ref lastNumOfRoundsClicked.ValueRW);
            var changeScreenBuffer = changeScreenEventBufferQuery.GetSingletonBuffer<ChangeScreenEventBuffer>();
            changeScreenBuffer.Add(new ChangeScreenEventBuffer { ScreenType = ScreenType.NumOfRounds });
            UnityEngine.Debug.Log("[TitleScreenControllersSetup] | Skipping GameMenu");
        };
#endif

        var titleScreenControllers = SystemAPI.ManagedAPI.GetSingleton<TitleScreenControllers>();
        titleScreenControllers.TitleScreenController = new(titleScreen);
        titleScreenControllers.NumOfRoundsController = new(numOfRoundsScreen, changeScreenEventBufferQuery, roundsDataEventBufferQuery);
        titleScreenControllers.NumOfPlayersController = new(numberOfPlayersScreen, changeScreenEventBufferQuery, playersDataEventBufferQuery);
        titleScreenControllers.CharacterSelectionControler = new(characterSelectionScreen, new CharacterSelectionContext());

        titleScreenControllers.TitleScreenController.ShowScreen();
        titleScreenControllers.NumOfRoundsController.HideScreen();
        titleScreenControllers.NumOfPlayersController.HideScreen();
        titleScreenControllers.CharacterSelectionControler.HideScreen();

        titleScreenControllers.TitleScreenController.SetChangeScreenEventBufferQuery(changeScreenEventBufferQuery);
        titleScreenControllers.CharacterSelectionControler.SetChangeScreenEventBufferQuery(changeScreenEventBufferQuery);

        var charactersDataEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<CharacterSelectedEventBuffer>().Build();
        titleScreenControllers.CharacterSelectionControler.SetDataEventBufferQuery(charactersDataEventBufferQuery);
    }

#if UNITY_EDITOR
    public static void SetDataForGameToStart(
            ref LoginData loginData,
            ref DynamicBuffer<CharacterSelectedNameBuffer> buffer,
            ref LastNumberOfRoundsClicked lastNumberOfRoundsClicked
            )
    {
        loginData.NumberOfPlayers = 2;
        loginData.NumberOfRounds = 12;
        lastNumberOfRoundsClicked.Value = 12;
        buffer.Add(new CharacterSelectedNameBuffer { Name = "Avocado" });
        buffer.Add(new CharacterSelectedNameBuffer { Name = "Bird" });
    }
#endif

    public void OnUpdate(ref SystemState state)
    { }

    public void OnStopRunning(ref SystemState state)
    {
    }
}

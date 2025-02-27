using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUIVisualElementComponent : IComponentData
{
    public VisualElement gameScreenTopRoot;
    public VisualElement gameScreenBotRoot;
    public VisualElement rollContainer;
    public VisualElement youBoughtContainer;
    public VisualElement taxContainer;
    public VisualElement jailContainer;
    public VisualElement goToJailContainer;
    public VisualElement chanceContainer;
    public VisualElement goContainer;
    public VisualElement parkingContainer;
    public VisualElement treasureContainer;
    public Label playerNameLabel;
}

public partial struct GameUICanvasSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CanvasReferenceComponent>();
    }

    public void OnStartRunning(ref SystemState state)
    {
        var canvasReference = SystemAPI.ManagedAPI.GetSingleton<CanvasReferenceComponent>();
        var uiDocument = Object.Instantiate(canvasReference.uiDocumentGO).GetComponent<UIDocument>();

        var gameUIVisualElementComponent = new GameUIVisualElementComponent
        {
            gameScreenTopRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-top-container"),
            gameScreenBotRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-bottom-container"),
        };

        state.EntityManager.AddComponentObject(state.SystemHandle, gameUIVisualElementComponent);

        gameUIVisualElementComponent.rollContainer = gameUIVisualElementComponent.gameScreenBotRoot.Q<VisualElement>("roll-display-container");
        gameUIVisualElementComponent.youBoughtContainer = gameUIVisualElementComponent.gameScreenBotRoot.Q<VisualElement>("you-bought-display-container");
        gameUIVisualElementComponent.taxContainer = gameUIVisualElementComponent.gameScreenBotRoot.Q<VisualElement>("tax-display-container");
        gameUIVisualElementComponent.jailContainer = gameUIVisualElementComponent.gameScreenBotRoot.Q<VisualElement>("jail-display-container");
        gameUIVisualElementComponent.goToJailContainer = gameUIVisualElementComponent.gameScreenBotRoot.Q<VisualElement>("go-to-jail-display-container");
        gameUIVisualElementComponent.chanceContainer = gameUIVisualElementComponent.gameScreenBotRoot.Q<VisualElement>("chance-display-container");
        gameUIVisualElementComponent.goContainer = gameUIVisualElementComponent.gameScreenBotRoot.Q<VisualElement>("go-display-container");
        gameUIVisualElementComponent.parkingContainer = gameUIVisualElementComponent.gameScreenBotRoot.Q<VisualElement>("parking-display-container");
        gameUIVisualElementComponent.treasureContainer = gameUIVisualElementComponent.gameScreenBotRoot.Q<VisualElement>("treasure-display-container");

        gameUIVisualElementComponent.rollContainer.style.display = DisplayStyle.None;
        gameUIVisualElementComponent.youBoughtContainer.style.display = DisplayStyle.None;
        gameUIVisualElementComponent.taxContainer.style.display = DisplayStyle.None;
        gameUIVisualElementComponent.jailContainer.style.display = DisplayStyle.None;
        gameUIVisualElementComponent.goToJailContainer.style.display = DisplayStyle.None;
        gameUIVisualElementComponent.chanceContainer.style.display = DisplayStyle.None;
        gameUIVisualElementComponent.goContainer.style.display = DisplayStyle.None;
        gameUIVisualElementComponent.parkingContainer.style.display = DisplayStyle.None;
        gameUIVisualElementComponent.treasureContainer.style.display = DisplayStyle.None;
    }


    public void OnUpdate(ref SystemState state)
    {
        var canvasVisualElements = SystemAPI.ManagedAPI.GetComponent<GameUIVisualElementComponent>(state.SystemHandle);
        Label playerNameLabel = canvasVisualElements.gameScreenTopRoot.Query<Label>("player-name-label");

        foreach (var (turnComponent, nameComponent) in SystemAPI.Query<RefRO<TurnComponent>, RefRO<NameDataComponent>>())
        {
            if (turnComponent.ValueRO.IsCurrentActivePlayer)
            {
                playerNameLabel.text = nameComponent.ValueRO.Name.ToString();
            }
        }
    }

    public void OnStopRunning(ref SystemState state) { }
}

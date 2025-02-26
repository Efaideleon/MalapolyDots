using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public partial struct GameUICanvasSystem : ISystem
{
    VisualElement gameScreenTopRoot;
    VisualElement gameScreenBotRoot;
    VisualElement rollContainer;
    VisualElement youBoughtContainer;
    VisualElement taxContainer;
    VisualElement jailContainer;
    VisualElement goToJailContainer;
    VisualElement chanceContainer;
    VisualElement goContainer;
    VisualElement parkingContainer;
    VisualElement treasureContainer;
    Label playerNameLabel;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CanvasReferenceComponent>();
        var canvasReference = SystemAPI.ManagedAPI.GetSingleton<CanvasReferenceComponent>();

        var uiDocument = Object.Instantiate(canvasReference.uiDocumentGO).GetComponent<UIDocument>();

        gameScreenTopRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-top-container");
        gameScreenBotRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-bottom-container");

        rollContainer = gameScreenBotRoot.Q<VisualElement>("roll-display-container");
        youBoughtContainer = gameScreenBotRoot.Q<VisualElement>("you-bought-display-container");
        taxContainer = gameScreenBotRoot.Q<VisualElement>("tax-display-container");
        jailContainer = gameScreenBotRoot.Q<VisualElement>("jail-display-container");
        goToJailContainer = gameScreenBotRoot.Q<VisualElement>("go-to-jail-display-container");
        chanceContainer = gameScreenBotRoot.Q<VisualElement>("chance-display-container");
        goContainer = gameScreenBotRoot.Q<VisualElement>("go-display-container");
        parkingContainer = gameScreenBotRoot.Q<VisualElement>("parking-display-container");
        treasureContainer = gameScreenBotRoot.Q<VisualElement>("treasure-display-container");

        playerNameLabel = gameScreenTopRoot.Q<Label>("player-name-label");

        rollContainer.style.display = DisplayStyle.None;
        youBoughtContainer.style.display = DisplayStyle.None;
        taxContainer.style.display = DisplayStyle.None;
        jailContainer.style.display = DisplayStyle.None;
        goToJailContainer.style.display = DisplayStyle.None;
        chanceContainer.style.display = DisplayStyle.None;
        goContainer.style.display = DisplayStyle.None;
        parkingContainer.style.display = DisplayStyle.None;
        treasureContainer.style.display = DisplayStyle.None;
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (turnComponent, nameComponent) in SystemAPI.Query<RefRO<TurnComponent>, RefRO<NameDataComponent>>())
        {
            if (turnComponent.ValueRO.IsCurrentActivePlayer)
            {
                playerNameLabel.text = nameComponent.ValueRO.Name.ToString();
            }
        }
    }
}

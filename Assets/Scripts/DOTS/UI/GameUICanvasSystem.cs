using System.IO.Pipes;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UIElements;

public partial struct GameUICanvasSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CanvasReferenceComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;
        var canvasReference = SystemAPI.ManagedAPI.GetSingleton<CanvasReferenceComponent>();

        var uiDocument = Object.Instantiate(canvasReference.uiDocumentGO).GetComponent<UIDocument>();

        VisualElement gameScreenTopRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-top-container");
        VisualElement gameScreenBotRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-bottom-container");

        Label playerNameLabel = gameScreenTopRoot.Q<Label>("player-name-label");

        VisualElement rollContainer = gameScreenBotRoot.Q<VisualElement>("roll-display-container");
        VisualElement youBoughtContainer = gameScreenBotRoot.Q<VisualElement>("you-bought-display-container");
        VisualElement taxContainer = gameScreenBotRoot.Q<VisualElement>("tax-display-container");
        VisualElement jailContainer = gameScreenBotRoot.Q<VisualElement>("jail-display-container");
        VisualElement goToJailContainer = gameScreenBotRoot.Q<VisualElement>("go-to-jail-display-container");
        VisualElement chanceContainer = gameScreenBotRoot.Q<VisualElement>("chance-display-container");
        VisualElement goContainer = gameScreenBotRoot.Q<VisualElement>("go-display-container");
        VisualElement parkingContainer = gameScreenBotRoot.Q<VisualElement>("parking-display-container");
        VisualElement treasureContainer = gameScreenBotRoot.Q<VisualElement>("treasure-display-container");

        rollContainer.style.display = DisplayStyle.None;
        youBoughtContainer.style.display = DisplayStyle.None;
        taxContainer.style.display = DisplayStyle.None;
        jailContainer.style.display = DisplayStyle.None;
        goToJailContainer.style.display = DisplayStyle.None;
        chanceContainer.style.display = DisplayStyle.None;
        goContainer.style.display = DisplayStyle.None;
        parkingContainer.style.display = DisplayStyle.None;
        treasureContainer.style.display = DisplayStyle.None;

        foreach (var (turnComponent, nameComponent) in SystemAPI.Query<RefRO<TurnComponent>, RefRO<NameDataComponent>>())
        {
            if (turnComponent.ValueRO.IsCurrentActivePlayer)
            {
                playerNameLabel.text = nameComponent.ValueRO.Name.ToString();
            }
        }
    }
}

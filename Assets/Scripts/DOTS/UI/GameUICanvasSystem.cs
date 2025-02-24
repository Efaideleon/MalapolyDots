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

        playerNameLabel.text = "ABEL";

    }
}

using Unity.Entities;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public struct RollAmountComponent : IComponentData
{
    public int Amount;
}

public class GameUIElementsComponent : IComponentData
{
    public VisualElement TopPanelRoot;
    public VisualElement BotPanelRoot;
    public VisualElement RollPanel;
    public VisualElement YouBoughtPanel;
    public VisualElement TaxPanel;
    public VisualElement JailPanel;
    public VisualElement GoToJailPanel;
    public VisualElement ChancePanel;
    public VisualElement GoPanel;
    public VisualElement ParkingPanel;
    public VisualElement TreasurePanel;
    public Label PlayerNameLabel;
    public Button RollButton;
    public Action OnRollButton;
    public Label RollLabel;
}

public partial struct GameUICanvasSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CanvasReferenceComponent>();
        state.RequireForUpdate<GameStateComponent>();
    }

    public void OnStartRunning(ref SystemState state)
    {
        var canvasRef = SystemAPI.ManagedAPI.GetSingleton<CanvasReferenceComponent>();
        var uiDocument = UnityEngine.Object.Instantiate(canvasRef.uiDocumentGO).GetComponent<UIDocument>();

        var gameUIElementsComponent = new GameUIElementsComponent
        {
            TopPanelRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-top-container"),
            BotPanelRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-bottom-container"),
        };

        state.EntityManager.AddComponentObject(state.SystemHandle, gameUIElementsComponent);

        gameUIElementsComponent.RollPanel = gameUIElementsComponent.BotPanelRoot.Q<VisualElement>("roll-display-container");
        gameUIElementsComponent.YouBoughtPanel = gameUIElementsComponent.BotPanelRoot.Q<VisualElement>("you-bought-display-container");
        gameUIElementsComponent.TaxPanel = gameUIElementsComponent.BotPanelRoot.Q<VisualElement>("tax-display-container");
        gameUIElementsComponent.JailPanel = gameUIElementsComponent.BotPanelRoot.Q<VisualElement>("jail-display-container");
        gameUIElementsComponent.GoToJailPanel = gameUIElementsComponent.BotPanelRoot.Q<VisualElement>("go-to-jail-display-container");
        gameUIElementsComponent.ChancePanel = gameUIElementsComponent.BotPanelRoot.Q<VisualElement>("chance-display-container");
        gameUIElementsComponent.GoPanel = gameUIElementsComponent.BotPanelRoot.Q<VisualElement>("go-display-container");
        gameUIElementsComponent.ParkingPanel = gameUIElementsComponent.BotPanelRoot.Q<VisualElement>("parking-display-container");
        gameUIElementsComponent.TreasurePanel = gameUIElementsComponent.BotPanelRoot.Q<VisualElement>("treasure-display-container");
        gameUIElementsComponent.RollLabel = gameUIElementsComponent.BotPanelRoot.Q<Label>("roll-amount-label");
        gameUIElementsComponent.RollButton = gameUIElementsComponent.RollPanel.Q<Button>("roll-button");

        gameUIElementsComponent.RollPanel.style.display = DisplayStyle.None;
        gameUIElementsComponent.YouBoughtPanel.style.display = DisplayStyle.None;
        gameUIElementsComponent.TaxPanel.style.display = DisplayStyle.None;
        gameUIElementsComponent.JailPanel.style.display = DisplayStyle.None;
        gameUIElementsComponent.GoToJailPanel.style.display = DisplayStyle.None;
        gameUIElementsComponent.ChancePanel.style.display = DisplayStyle.None;
        gameUIElementsComponent.GoPanel.style.display = DisplayStyle.None;
        gameUIElementsComponent.ParkingPanel.style.display = DisplayStyle.None;
        gameUIElementsComponent.TreasurePanel.style.display = DisplayStyle.None;

        var rollAmountEntity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<RollAmountComponent>(),
        });

        var rollAmountComponent = new RollAmountComponent
        {
            Amount = 0,
        };

        SystemAPI.SetComponent(rollAmountEntity, rollAmountComponent);

        gameUIElementsComponent.OnRollButton = () =>
        {
            rollAmountComponent.Amount = UnityEngine.Random.Range(1, 6);
            gameUIElementsComponent.RollLabel.text = rollAmountComponent.Amount.ToString();
            gameUIElementsComponent.RollButton.style.display = DisplayStyle.None;
        };
        gameUIElementsComponent.RollButton.clickable.clicked += gameUIElementsComponent.OnRollButton;
    }

    public void OnUpdate(ref SystemState state)
    {
        var canvasVisualElements = SystemAPI.ManagedAPI.GetComponent<GameUIElementsComponent>(state.SystemHandle);
        Label playerNameLabel = canvasVisualElements.TopPanelRoot.Query<Label>("player-name-label");

        foreach (var (turnComponent, nameComponent) in SystemAPI.Query<RefRO<TurnComponent>, RefRO<NameDataComponent>>())
        {
            if (turnComponent.ValueRO.IsActive)
            {
                playerNameLabel.text = nameComponent.ValueRO.Name.ToString();
            }
        }

        foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
        {
            switch (gameState.ValueRO.State)
            {
                case GameState.Rolling:
                    canvasVisualElements.RollPanel.style.display = DisplayStyle.Flex;
                    Debug.Log("showing roll button");
                    break;
            }
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
        var canvasVisualElements = SystemAPI.ManagedAPI.GetComponent<GameUIElementsComponent>(state.SystemHandle);
        canvasVisualElements.RollButton.clickable.clicked -= canvasVisualElements.OnRollButton;
    }
}


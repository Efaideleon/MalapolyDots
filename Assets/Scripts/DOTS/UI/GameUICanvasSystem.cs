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

        gameUIElementsComponent.RollPanel = gameUIElementsComponent.BotPanelRoot.Q("RollPanel");
        gameUIElementsComponent.YouBoughtPanel = gameUIElementsComponent.BotPanelRoot.Q("PopupMenuPanel");
        gameUIElementsComponent.TaxPanel = gameUIElementsComponent.BotPanelRoot.Q("TaxPanel");
        gameUIElementsComponent.JailPanel = gameUIElementsComponent.BotPanelRoot.Q("JailPanel");
        gameUIElementsComponent.GoToJailPanel = gameUIElementsComponent.BotPanelRoot.Q("GoToJailPanel");
        gameUIElementsComponent.ChancePanel = gameUIElementsComponent.BotPanelRoot.Q("ChancePanel");
        gameUIElementsComponent.GoPanel = gameUIElementsComponent.BotPanelRoot.Q("GoPanel");
        gameUIElementsComponent.ParkingPanel = gameUIElementsComponent.BotPanelRoot.Q("ParkingPanel");
        gameUIElementsComponent.TreasurePanel = gameUIElementsComponent.BotPanelRoot.Q("TreasurePanel");

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

        SystemAPI.SetComponent(rollAmountEntity, new RollAmountComponent
        {
            Amount = 0,
        });

        var rollAmountComponent = SystemAPI.QueryBuilder().WithAllRW<RollAmountComponent>().Build();

        gameUIElementsComponent.OnRollButton = () =>
        {
            var numRolled = UnityEngine.Random.Range(1, 6);
            rollAmountComponent.GetSingletonRW<RollAmountComponent>().ValueRW.Amount = numRolled;
            gameUIElementsComponent.RollLabel.text = numRolled.ToString();
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
                playerNameLabel.text = nameComponent.ValueRO.Value.ToString();
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


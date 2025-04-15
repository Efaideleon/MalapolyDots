using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public enum TransactionEventsEnum
{
    Purchase,
    ChangeTurn,
    PayRent,
    UpgradeHouse,
    Default
}

public struct LastPropertyClicked : IComponentData
{
    public Entity entity;
}

public struct TransactionEvent
{
    public TransactionEventsEnum EventType;
}

public struct TransactionEventBus : IComponentData
{
    public NativeQueue<TransactionEvent> EventQueue;
}

public struct RollAmountComponent : IComponentData
{
    public int Amount;
}

public class OverLayPanels : IComponentData
{
    public StatsPanel statsPanel;
    public RollPanel rollPanel;
    public PurchaseHousePanel purchaseHousePanel;
}

public class PanelControllers : IComponentData
{
    public PurchasePanelController purchasePanelController;
    public SpaceActionsPanelController spaceActionsPanelController;
    public BackdropController backdropController;
}

public class OnLandPanelsDictionay : IComponentData
{
    public Dictionary<SpaceTypeEnum, OnLandPanel> Value;
}

public partial struct GameUICanvasSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        UnityEngine.Debug.Log(">>> GameUICanvasSystem.OnCreate CALLED <<<");
        state.RequireForUpdate<CanvasReferenceComponent>();
        state.RequireForUpdate<GameStateComponent>();
        state.RequireForUpdate<CurrentPlayerID>();
        state.RequireForUpdate<LandedOnSpace>();
        state.RequireForUpdate<MoneyComponent>();
        state.RequireForUpdate<OverLayPanels>();
        state.RequireForUpdate<PanelControllers>();
        state.RequireForUpdate<ClickData>();
        state.RequireForUpdate<ClickedPropertyComponent>();
        state.RequireForUpdate<LastPropertyClicked>();

        state.EntityManager.CreateSingleton( new LastPropertyClicked { entity = Entity.Null });

        // OverLayPanels Entity
        var uiEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentObject(uiEntity, new OverLayPanels
        {
            rollPanel = null,
            statsPanel = null,
            purchaseHousePanel = null
        });

        // PanelControllers Entity
        var controllerEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentObject(controllerEntity, new PanelControllers
        {
            purchasePanelController = null,
            spaceActionsPanelController = null
        });

        // TransactionEvents Entity
        EntityQuery query = state.GetEntityQuery(ComponentType.ReadOnly<TransactionEventBus>());
        if (query.IsEmpty)
        {
            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<TransactionEventBus>()
            });
            SystemAPI.SetComponent(entity, new TransactionEventBus
            {
                EventQueue = new NativeQueue<TransactionEvent>(Allocator.Persistent)
            });
        }
        else
        {
            var entity = query.GetSingletonEntity();
            var events = state.EntityManager.GetComponentData<TransactionEventBus>(entity);

            if (events.EventQueue.IsCreated)
            {
                events.EventQueue.Dispose();
                events.EventQueue = new NativeQueue<TransactionEvent>(Allocator.Persistent);
                state.EntityManager.SetComponentData(entity, events);
            }
        }
    }

    public void OnStartRunning(ref SystemState state)
    {
        UnityEngine.Debug.Log(">>> GameUICanvasSystem.OnStartRunning CALLED <<<");
        var canvasRef = SystemAPI.ManagedAPI.GetSingleton<CanvasReferenceComponent>();
        if (canvasRef.uiDocumentGO == null)
        {
            UnityEngine.Debug.LogError("CanvasReferenceComponent has a null uiDocumentGO! Cannot instantiate UI.");
            return; // Or disable system state.Enabled = false;
        }
        UnityEngine.Debug.Log($">>> CanvasReferenceComponent Found. Prefab GO: {canvasRef.uiDocumentGO.name}");

        // --- Instantiate Prefab ---
        var uiGameObject = UnityEngine.Object.Instantiate(canvasRef.uiDocumentGO);
        var uiDocument = uiGameObject.GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            UnityEngine.Debug.LogError($"Instantiated UI Prefab '{uiGameObject.name}' is missing UIDocument component!");
            UnityEngine.Object.Destroy(uiGameObject); // Clean up useless instance
            return; // Or disable system
        }

        UnityEngine.Debug.Log($">>> Instantiated UIDocument '{uiGameObject.name}'. Initial rootVisualElement is {(uiDocument.rootVisualElement == null ? "NULL" : "NOT NULL")}");

        var topPanelRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-top-container");
        var botPanelRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-bottom-container");
        var backdrop = uiDocument.rootVisualElement.Q<Button>("Backdrop");

        PurchaseHousePanelContext purchaseHousePanelContext = new ()
        {
            Name = default,
            HousesOwned = default,
            Price = default
        };

        SpaceActionsPanelContext spaceActionsPanelContext = new ()
        {
            HasMonopoly = false,
            IsPlayerOwner = false
        };

        StatsPanel statsPanel = new(topPanelRoot);
        RollPanel rollPanel = new(botPanelRoot);
        SpaceActionsPanel spaceActionsPanel = new(botPanelRoot);
        PurchaseHousePanel purchaseHousePanel = new(botPanelRoot, purchaseHousePanelContext);
        NoMonopolyYetPanel noMonopolyYetPanel = new(botPanelRoot);

        // Register the panels to hide, such as the ActionsSpace Panel

        // why do we have uiPanels?
        // To put it in components and change them on the OnUpdate loop
        var uiEntity = SystemAPI.ManagedAPI.GetSingleton<OverLayPanels>();
        uiEntity.rollPanel = rollPanel;
        uiEntity.statsPanel = statsPanel;
        uiEntity.purchaseHousePanel = purchaseHousePanel;

        // Loading BuyHouseUIController component.
        var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
        panelControllers.purchasePanelController = new(purchaseHousePanel);
        panelControllers.spaceActionsPanelController = new(spaceActionsPanelContext, spaceActionsPanel, purchaseHousePanel, noMonopolyYetPanel);
        panelControllers.backdropController = new(backdrop);
        panelControllers.backdropController.RegisterPanelToHide(spaceActionsPanel.Panel);
        panelControllers.backdropController.RegisterPanelToHide(purchaseHousePanel.Panel);

        // Setting Dictionary for each SpaceType to Panel;
        Dictionary<SpaceTypeEnum, OnLandPanel> onLandPanelsDictionary = new()
        {
            { SpaceTypeEnum.Property, new PropertyPanel(botPanelRoot) },
            { SpaceTypeEnum.Tax, new TaxPanel(botPanelRoot) },
            { SpaceTypeEnum.Jail, new JailPanel(botPanelRoot) },
            { SpaceTypeEnum.GoToJail, new GoToJailPanel(botPanelRoot) },
            { SpaceTypeEnum.Chance, new ChancePanel(botPanelRoot) },
            { SpaceTypeEnum.Go, new GoPanel(botPanelRoot) },
            { SpaceTypeEnum.Parking, new ParkingPanel(botPanelRoot) },
            { SpaceTypeEnum.Treasure, new TreasurePanel(botPanelRoot) },
        };

        state.EntityManager.AddComponentObject(state.SystemHandle, new OnLandPanelsDictionay
        {
            Value = onLandPanelsDictionary
        });

        // Create RollAmountComponent
        var rollAmountEntity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<RollAmountComponent>(),
        });

        SystemAPI.SetComponent(rollAmountEntity, new RollAmountComponent { Amount = 0 });

        // Button Actions
        var rollAmountComponent = SystemAPI.QueryBuilder().WithAllRW<RollAmountComponent>().Build();
        rollPanel.AddActionToRollButton(() =>
        {
            var valueRolled = UnityEngine.Random.Range(1, 6);
            rollAmountComponent.GetSingletonRW<RollAmountComponent>().ValueRW.Amount = valueRolled;
            rollPanel.UpdateRollLabel(valueRolled.ToString());
            rollPanel.HideRollButton();
        });

        // Events Bus
        var transactionEventsQuery = SystemAPI.QueryBuilder().WithAllRW<TransactionEventBus>().Build();
        var buyHouseEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<BuyHouseEvent>().Build();
        panelControllers.purchasePanelController.SetBuyHouseEventQuery(buyHouseEventBufferQuery);

        foreach (var onLandPanel in onLandPanelsDictionary.Values)
        {
            onLandPanel.AddAcceptButtonAction(transactionEventsQuery);
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        var overlayPanels = SystemAPI.ManagedAPI.GetSingleton<OverLayPanels>();
        var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>(); 

        foreach (var currPlayerID in SystemAPI.Query<RefRO<CurrentPlayerID>>().WithChangeFilter<CurrentPlayerID>())
        {
            foreach (var (playerID, name, money) in 
                    SystemAPI.Query<
                        RefRO<PlayerID>,
                        RefRO<NameComponent>,
                        RefRO<MoneyComponent>
                    >())
            {
                if (playerID.ValueRO.Value == currPlayerID.ValueRO.Value)
                {
                    overlayPanels.statsPanel.UpdatePlayerNameLabelText(name.ValueRO.Value.ToString());
                    overlayPanels.statsPanel.UpdatePlayerMoneyLabelText(money.ValueRO.Value.ToString());
                }
            }
        }

        foreach (var (playerID, money) in 
                SystemAPI.Query<RefRO<PlayerID>, RefRO<MoneyComponent>>().WithChangeFilter<MoneyComponent>())
        {
            var currPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();
            if (playerID.ValueRO.Value == currPlayerID.Value)
            {
                overlayPanels.statsPanel.UpdatePlayerMoneyLabelText(money.ValueRO.Value.ToString());
            }
        }
        
        foreach ( var _ in SystemAPI.Query<RefRO<MonopolyFlagComponent>>().WithChangeFilter<MonopolyFlagComponent>())
        {
            var lastPropertyClicked =  SystemAPI.GetSingleton<LastPropertyClicked>();
            if (lastPropertyClicked.entity != Entity.Null)
            {
                var hasMonopoly = SystemAPI.GetComponent<MonopolyFlagComponent>(lastPropertyClicked.entity);
                var owner = SystemAPI.GetComponent<OwnerComponent>(lastPropertyClicked.entity);
                var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();

                bool isCurrentOwner = false;
                if (currentPlayerID.Value == owner.ID)
                {
                    isCurrentOwner = true;
                }
                SpaceActionsPanelContext spaceActionsContext = new()
                {
                    HasMonopoly = hasMonopoly.Value,
                    IsPlayerOwner = isCurrentOwner
                };

                panelControllers.spaceActionsPanelController.Context = spaceActionsContext;
                continue;
            }
        }

        // When an entity is clicked show the panel to buy houses
        foreach ( var clickedProperty in SystemAPI.Query<RefRW<ClickedPropertyComponent>>().WithChangeFilter<ClickedPropertyComponent>())
        {
            UnityEngine.Debug.Log("Click recieved in GameUICanvasSystem");
            if (clickedProperty.ValueRO.entity != Entity.Null)
            {
                //show and hide the purchase panel here
                UnityEngine.Debug.Log("clicked property entity is not null");
                var clickData = SystemAPI.GetSingleton<ClickData>();
                SystemAPI.SetSingleton(new LastPropertyClicked { entity = clickedProperty.ValueRO.entity });

                switch (clickData.Phase)
                {
                    case InputActionPhase.Started:
                        PurchaseHousePanelContext context = new()
                        {
                            Name = SystemAPI.GetComponent<NameComponent>(clickedProperty.ValueRO.entity).Value,
                            HousesOwned = SystemAPI.GetComponent<HouseCount>(clickedProperty.ValueRO.entity).Value,
                            Price = 10,
                        };

                        // TODO: Should I create a proxy function to this?
                        // TODO: Should I Set the Context and then call Update or do it in one Call Function?
                        panelControllers.purchasePanelController.PurchaseHousePanel.Context = context;
                        panelControllers.purchasePanelController.PurchaseHousePanel.Update();
                        panelControllers.spaceActionsPanelController.SpaceActionsPanel.Show();
                        UnityEngine.Debug.Log("Click Started showing space actions panel");
                        break;
                    case InputActionPhase.Canceled:
                        panelControllers.backdropController.ShowBackdrop();
                        UnityEngine.Debug.Log("Click Canceled showing backdrop panel");
                        break;
                }
                // TODO: The backdrop panel should appear whenever one of the hideable panels is appears.
                SystemAPI.SetSingleton( new ClickedPropertyComponent { entity = Entity.Null });
            }
        }

        foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
        {
            switch (gameState.ValueRO.State)
            {
                case GameState.Rolling:
                    overlayPanels.rollPanel.Show();
                    break;
                case GameState.Landing: 
                    overlayPanels.rollPanel.Hide();

                    var spaceLanded = SystemAPI.GetSingleton<LandedOnSpace>();
                    var spaceLandedType = SystemAPI
                        .GetComponent<SpaceTypeComponent>(spaceLanded.entity)
                        .Value;
                    var landPanels = SystemAPI
                        .ManagedAPI
                        .GetComponent<OnLandPanelsDictionay>(state.SystemHandle)
                        .Value;

                    // Get the correct popup panel to show.
                    var landPanel = landPanels[spaceLandedType];
                    var playerID = SystemAPI.GetSingleton<CurrentPlayerID>();
                    var context = new ShowPanelContext 
                    {
                        entityManager = state.EntityManager,
                        spaceEntity = spaceLanded.entity,
                        playerID = playerID.Value
                    };
                    // This landPanel is more like a manager that will determine the correct panel
                    // to show for the given space type
                    landPanel.Show(context);
                    break;
            }
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
        // TODO: Rename uiPanels for consistency
        var uiPanels = SystemAPI.ManagedAPI.GetSingleton<OverLayPanels>();
        uiPanels.rollPanel.Dispose();
        var panelsController = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
        panelsController.purchasePanelController.Dispose();
        panelsController.spaceActionsPanelController.Dispose();
        panelsController.backdropController.Dispose();

        // First unsubscribe the button.clicked event 
        var onLandPanelsDictionary = state
            .EntityManager
            .GetComponentData<OnLandPanelsDictionay>(state.SystemHandle)
            .Value;

        foreach (var onLandPanel in onLandPanelsDictionary.Values)
        {
            onLandPanel.Dispose();
        }
    }

    //Is there a chance that the OnDestroy method from a system runs before the OnStopRunning of another?
    public void OnDestroy(ref SystemState state)
    {
        // Then free the TransactionEvents.EventQueue since no anonymous function has a handle to the NativeQueue.
        var query = state.GetEntityQuery(ComponentType.ReadOnly<TransactionEventBus>());
        foreach (var entity in query.ToEntityArray(Allocator.Temp))
        {
            var transactionEvent = state.EntityManager.GetComponentData<TransactionEventBus>(entity);
            if (transactionEvent.EventQueue.IsCreated)
            {
                transactionEvent.EventQueue.Dispose();
                transactionEvent.EventQueue = default;
                state.EntityManager.SetComponentData(entity, transactionEvent);
            }
        }
    }
}


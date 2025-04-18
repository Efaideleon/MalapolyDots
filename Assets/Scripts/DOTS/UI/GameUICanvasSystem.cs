using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public enum TransactionEventType
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
    public TransactionEventType EventType;
}

public struct TransactionEventBus : IComponentData
{
    public NativeQueue<TransactionEvent> EventQueue;
}

public struct RollAmountComponent : IComponentData
{
    public int Amount;
}

public class OverlayPanels : IComponentData
{
    public StatsPanel statsPanel;
    public RollPanel rollPanel;
    public PurchaseHousePanel purchaseHousePanel;
}

public class PanelControllers : IComponentData
{
    public PurchaseHousePanelController purchaseHousePanelController;
    public SpaceActionsPanelController spaceActionsPanelController;
    public BackdropController backdropController;
    public PurchasePropertyPanelController purchasePropertyPanelController;
    public PayRentPanelController payRentPanelController;
}

public class PopupManagers : IComponentData
{
    public PropertyPopupManager propertyPopupManager;
}

public class OnLandPanelsDictionay : IComponentData
{
    public Dictionary<SpaceType, OnLandPanel> Value;
}

public partial struct GameUICanvasSystem : ISystem, ISystemStartStop
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CanvasReferenceComponent>();
        state.RequireForUpdate<GameStateComponent>();
        state.RequireForUpdate<CurrentPlayerID>();
        state.RequireForUpdate<LandedOnSpace>();
        state.RequireForUpdate<MoneyComponent>();
        state.RequireForUpdate<OverlayPanels>();
        state.RequireForUpdate<PanelControllers>();
        state.RequireForUpdate<PopupManagers>();
        state.RequireForUpdate<ClickData>();
        state.RequireForUpdate<ClickedPropertyComponent>();
        state.RequireForUpdate<LastPropertyClicked>();
        state.RequireForUpdate<PurchasePropertyPanelContextComponent>();
        state.RequireForUpdate<PurhcaseHousePanelContextComponent>();

        state.EntityManager.CreateSingleton(new LastPropertyClicked { entity = Entity.Null });
        state.EntityManager.CreateSingleton(new OverlayPanels { rollPanel = null, statsPanel = null, purchaseHousePanel = null });
        state.EntityManager.CreateSingleton(new PanelControllers { purchaseHousePanelController = null, spaceActionsPanelController = null });
        state.EntityManager.CreateSingleton(new PopupManagers { propertyPopupManager = null});

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
            return; // Or disable system state.Enabled = false;
        }

        // --- Instantiate Prefab ---
        var uiGameObject = UnityEngine.Object.Instantiate(canvasRef.uiDocumentGO);
        var uiDocument = uiGameObject.GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            UnityEngine.Object.Destroy(uiGameObject); // Clean up useless instance
            return; // Or disable system
        }

        var topPanelRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-top-container");
        var botPanelRoot = uiDocument.rootVisualElement.Q<VisualElement>("game-screen-bottom-container");
        var backdrop = uiDocument.rootVisualElement.Q<Button>("Backdrop");

        PurchasePropertyPanelContext purchasePropertyPanelContext = new()
        {
            Name = default,
            Price = default,
        };

        PurchaseHousePanelContext purchaseHousePanelContext = new()
        {
            Name = default,
            HousesOwned = default,
            Price = default
        };

        SpaceActionsPanelContext spaceActionsPanelContext = new()
        {
            HasMonopoly = false,
            IsPlayerOwner = false
        };

        PayRentPanelContext payRentPanelContext = new()
        {
            Rent = default
        };

        StatsPanel statsPanel = new(topPanelRoot);
        RollPanel rollPanel = new(botPanelRoot);
        SpaceActionsPanel spaceActionsPanel = new(botPanelRoot);
        PurchaseHousePanel purchaseHousePanel = new(botPanelRoot, purchaseHousePanelContext);
        NoMonopolyYetPanel noMonopolyYetPanel = new(botPanelRoot);
        PurchasePropertyPanel purchasePropertyPanel = new(botPanelRoot);
        PayRentPanel payRentPanel = new(botPanelRoot);

        // Register the panels to hide, such as the ActionsSpace Panel
        // why do we have uiPanels?
        // To put it in components and change them on the OnUpdate loop
        var uiEntity = SystemAPI.ManagedAPI.GetSingleton<OverlayPanels>();
        uiEntity.rollPanel = rollPanel;
        uiEntity.statsPanel = statsPanel;
        uiEntity.purchaseHousePanel = purchaseHousePanel;

        // Loading Controllers
        var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();

        panelControllers.backdropController = new(backdrop);
        panelControllers.backdropController.RegisterPanelToHide(spaceActionsPanel.Panel);
        panelControllers.backdropController.RegisterPanelToHide(purchaseHousePanel.Panel);
        panelControllers.backdropController.RegisterPanelToHide(noMonopolyYetPanel.Panel);
        panelControllers.backdropController.RegisterPanelToHide(purchasePropertyPanel.Panel);

        panelControllers.purchaseHousePanelController = new(purchaseHousePanel);
        panelControllers.purchasePropertyPanelController = new(purchasePropertyPanel, purchasePropertyPanelContext);
        panelControllers.payRentPanelController = new(payRentPanel, payRentPanelContext);

        panelControllers.spaceActionsPanelController = new(
                spaceActionsPanelContext,
                spaceActionsPanel,
                panelControllers.purchaseHousePanelController,
                noMonopolyYetPanel,
                panelControllers.purchasePropertyPanelController);

        PropertyPopupManagerContext propertyPopupManagerContext = new()
        {
            OwnerID = default,
            CurrentPlayerID = default
        };
        PropertyPopupManager propertyPopupManager = new(payRentPanel, propertyPopupManagerContext);
        SystemAPI.ManagedAPI.GetSingleton<PopupManagers>().propertyPopupManager = propertyPopupManager;
        // Setting Dictionary for each SpaceType to Panel;
        Dictionary<SpaceType, OnLandPanel> onLandPanelsDictionary = new()
        {
            { SpaceType.Tax, new TaxPanel(botPanelRoot) },
            { SpaceType.Jail, new JailPanel(botPanelRoot) },
            { SpaceType.GoToJail, new GoToJailPanel(botPanelRoot) },
            { SpaceType.Chance, new ChancePanel(botPanelRoot) },
            { SpaceType.Go, new GoPanel(botPanelRoot) },
            { SpaceType.Parking, new ParkingPanel(botPanelRoot) },
            { SpaceType.Treasure, new TreasurePanel(botPanelRoot) },
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
            // var valueRolled = UnityEngine.Random.Range(1, 6);
            var valueRolled = 1;
            rollAmountComponent.GetSingletonRW<RollAmountComponent>().ValueRW.Amount = valueRolled;
            rollPanel.UpdateRollLabel(valueRolled.ToString());
            rollPanel.HideRollButton();
        });

        // Events Bus
        var transactionEventsQuery = SystemAPI.QueryBuilder().WithAllRW<TransactionEventBus>().Build();
        var buyHouseEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<BuyHouseEvent>().Build();
        panelControllers.purchaseHousePanelController.SetBuyHouseEventQuery(buyHouseEventBufferQuery);
        panelControllers.purchasePropertyPanelController.SetTransactionEventQuery(transactionEventsQuery);
        panelControllers.payRentPanelController.SetTransactionEventBusQuery(transactionEventsQuery);

        foreach (var onLandPanel in onLandPanelsDictionary.Values)
        {
            onLandPanel.AddAcceptButtonAction(transactionEventsQuery);
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        // Every frame we are accessing this managed components maybe move this each foreach loop?
        var overlayPanels = SystemAPI.ManagedAPI.GetSingleton<OverlayPanels>();
        var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();

        foreach (var spaceActionsContext in
                SystemAPI.Query<
                    RefRO<SpaceActionsPanelContextComponent>
                >()
                .WithChangeFilter<SpaceActionsPanelContextComponent>())
        {
            panelControllers.spaceActionsPanelController.Context = spaceActionsContext.ValueRO.Value;
        }

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

        foreach (var purchaseHousePanelContext in
                SystemAPI.Query<
                    RefRO<PurhcaseHousePanelContextComponent>
                >()
                .WithChangeFilter<PurhcaseHousePanelContextComponent>())
        {
            if (panelControllers.purchaseHousePanelController != null)
            {
                panelControllers.purchaseHousePanelController.PurchaseHousePanel.Context = purchaseHousePanelContext.ValueRO.Value;
                panelControllers.purchaseHousePanelController.PurchaseHousePanel.Update();
            }
        }

        foreach (var purchasePropertyPanelContext in
                SystemAPI.Query<
                    RefRO<PurchasePropertyPanelContextComponent>
                >()
                .WithChangeFilter<PurchasePropertyPanelContextComponent>())
        {
            if (panelControllers.purchaseHousePanelController != null)
            {
                // TODO: not consistent with the PurhcaseHousePanel.
                // Here we assigned the Context to the controller instead of the panel itself
                panelControllers.purchasePropertyPanelController.Context = purchasePropertyPanelContext.ValueRO.Value;
                panelControllers.purchasePropertyPanelController.Update();
            }
        }

        foreach (var payRentPanelContext in SystemAPI.Query<
                    RefRO<PayRentPanelContextComponent>
                >()
                .WithChangeFilter<PayRentPanelContextComponent>())
        {
            if (panelControllers.payRentPanelController != null)
            {
                panelControllers.payRentPanelController.Context = payRentPanelContext.ValueRO.Value;
                panelControllers.payRentPanelController.Update();
            }
        }

        // When an entity is clicked show the actions panel 
        foreach (var clickedProperty in
                SystemAPI.Query<
                    RefRW<ClickedPropertyComponent>
                >()
                .WithChangeFilter<ClickedPropertyComponent>())
        {
            if (clickedProperty.ValueRO.entity != Entity.Null)
            {
                var clickData = SystemAPI.GetSingleton<ClickData>();
                SystemAPI.SetSingleton(new LastPropertyClicked { entity = clickedProperty.ValueRO.entity });
                UnityEngine.Debug.Log("a property was clicked");

                switch (clickData.Phase)
                {
                    case InputActionPhase.Started:
                        panelControllers.spaceActionsPanelController.SpaceActionsPanel.Show();
                        break;
                    case InputActionPhase.Canceled:
                        panelControllers.backdropController.ShowBackdrop();
                        break;
                }
                // TODO: The backdrop panel should appear whenever one of the hideable panels is appears.
                SystemAPI.SetSingleton(new ClickedPropertyComponent { entity = Entity.Null });
            }
        }

        // Updates the context for the popup when the player lands on a property space.
        foreach (var landOnProperty in SystemAPI.Query<RefRO<LandedOnSpace>>().WithChangeFilter<LandedOnSpace>())
        {
            var landOnPropertyEntity = landOnProperty.ValueRO.entity;
            if (landOnPropertyEntity != null && SystemAPI.HasComponent<PropertySpaceTag>(landOnPropertyEntity))
            {
                var popupManagers = SystemAPI.ManagedAPI.GetSingleton<PopupManagers>();

                PropertyPopupManagerContext propertyPopupManagerContext = new ()
                {
                    OwnerID = SystemAPI.GetComponent<OwnerComponent>(landOnPropertyEntity).ID,
                    CurrentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>().Value
                };
                popupManagers.propertyPopupManager.Context = propertyPopupManagerContext;
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
                    if (SystemAPI.HasComponent<PropertySpaceTag>(spaceLanded.entity))
                    {
                        var popupManagers = SystemAPI.ManagedAPI.GetSingleton<PopupManagers>();
                        popupManagers.propertyPopupManager.TriggerPopup();
                    }

                    // Legacy Code
                    // var spaceLanded = SystemAPI.GetSingleton<LandedOnSpace>();
                    // var spaceLandedType = SystemAPI
                    //     .GetComponent<SpaceTypeComponent>(spaceLanded.entity)
                    //     .Value;
                    // var landPanels = SystemAPI
                    //     .ManagedAPI
                    //     .GetComponent<OnLandPanelsDictionay>(state.SystemHandle)
                    //     .Value;
                    //
                    // // Get the correct popup panel to show.
                    // var landPanel = landPanels[spaceLandedType];
                    // var playerID = SystemAPI.GetSingleton<CurrentPlayerID>();
                    // var context = new ShowPanelContext
                    // {
                    //     entityManager = state.EntityManager,
                    //     spaceEntity = spaceLanded.entity,
                    //     playerID = playerID.Value
                    // };
                    // // This landPanel is more like a manager that will determine the correct panel
                    // // to show for the given space type
                    // landPanel.Show(context);
                    break;
            }
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
        // TODO: Rename uiPanels for consistency
        var uiPanels = SystemAPI.ManagedAPI.GetSingleton<OverlayPanels>();
        uiPanels.rollPanel.Dispose();
        var panelsController = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
        panelsController.purchaseHousePanelController.Dispose();
        panelsController.spaceActionsPanelController.Dispose();
        panelsController.backdropController.Dispose();
        panelsController.purchasePropertyPanelController.Dispose();
        panelsController.payRentPanelController.Dispose();

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


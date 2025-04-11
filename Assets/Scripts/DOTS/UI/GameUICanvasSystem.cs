using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.UIElements;
using System.Collections.Generic;

public enum TransactionEventsEnum
{
    Purchase,
    ChangeTurn,
    PayRent,
    UpgradeHouse,
    Default
}

public struct TransactionEvent
{
    public TransactionEventsEnum EventType;
}

public struct TransactionEvents : IComponentData
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
}

public class PanelControllers : IComponentData
{
    public BuyHouseUIController buyHouseUIController;
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

        // OverLayPanels Entity
        var uiEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentObject(uiEntity, new OverLayPanels
        {
            rollPanel = null,
            statsPanel = null,
        });

        // PanelControllers Entity
        var controllerEntity = state.EntityManager.CreateEntity();
        state.EntityManager.AddComponentObject(uiEntity, new PanelControllers
        {
            buyHouseUIController = null,
        });

        // TransactionEvents Entity
        EntityQuery query = state.GetEntityQuery(ComponentType.ReadOnly<TransactionEvents>());
        if (query.IsEmpty)
        {
            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<TransactionEvents>()
            });
            SystemAPI.SetComponent(entity, new TransactionEvents
            {
                EventQueue = new NativeQueue<TransactionEvent>(Allocator.Persistent)
            });
        }
        else
        {
            var entity = query.GetSingletonEntity();
            var events = state.EntityManager.GetComponentData<TransactionEvents>(entity);

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

        StatsPanel statsPanel = new(topPanelRoot);
        RollPanel rollPanel = new(botPanelRoot);
        BuyHouseUI buyHouseUI = new(botPanelRoot);

        // why do we have uiPanels?
        // To put it in components and change them on the OnUpdate loop
        var uiPanels = new OverLayPanels
        {
            statsPanel = statsPanel,
            rollPanel = rollPanel,
        };

        foreach (var (overlayPanels, uiEntity) in SystemAPI.Query<OverLayPanels>().WithEntityAccess())
        {
            state.EntityManager.SetComponentData(uiEntity, uiPanels);
        }

        // Adding the PanelControllers IComponentData to an Entity;
        BuyHouseUIController buyHouseUIController = new(buyHouseUI);
        var panelControllers = new PanelControllers
        {
            buyHouseUIController = buyHouseUIController
        };
        foreach (var (controller, controllerEntity) in SystemAPI.Query<PanelControllers>().WithEntityAccess())
        {
            state.EntityManager.SetComponentData(controllerEntity, controller);
        }

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
        uiPanels.rollPanel.AddActionToRollButton(() =>
        {
            // var valueRolled = UnityEngine.Random.Range(1, 6);
            var valueRolled = 1;
            rollAmountComponent.GetSingletonRW<RollAmountComponent>().ValueRW.Amount = valueRolled;
            uiPanels.rollPanel.UpdateRollLabel(valueRolled.ToString());
            uiPanels.rollPanel.HideRollButton();
        });

        // Adding the transactionEventsQuery to the accept button in the ui
        // so that when they are clicked an event happens
        var transactionEventsQuery = SystemAPI.QueryBuilder().WithAllRW<TransactionEvents>().Build();
        // BUG?: I wonder if this gets the dynamic buffer
        var buyHouseEventsQuery = SystemAPI.QueryBuilder().WithAllRW<BuyHouseEvent>().Build();

        buyHouseUI.SetBuyHouseEventQuery(buyHouseEventsQuery);

        foreach (var onLandPanel in onLandPanelsDictionary.Values)
        {
            onLandPanel.AddAcceptButtonAction(transactionEventsQuery);
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        var overlayPanels = SystemAPI.ManagedAPI.GetSingleton<OverLayPanels>();

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

                    // Query for the properties that are a monopoly.
                    foreach (var (owner, monopoly, name) in 
                            SystemAPI.Query<
                                RefRO<OwnerComponent>,
                                RefRO<MonopolyFlagComponent>,
                                RefRO<NameComponent>
                            >())
                    {
                        if (owner.ValueRO.ID == playerID.Value && monopoly.ValueRO.State == true)
                        {
                            // Pass the name of the property where a house can be bought to ui here
                            overlayPanels.buyhouseUI.AddPropertyName(name.ValueRO.Value.ToString());
                        }
                    }
                    break;
            }
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
        // TODO: Rename uiPanels for consistency
        var uiPanels = SystemAPI.ManagedAPI.GetSingleton<OverLayPanels>();
        uiPanels.rollPanel.Dispose();

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
        var query = state.GetEntityQuery(ComponentType.ReadOnly<TransactionEvents>());
        foreach (var entity in query.ToEntityArray(Allocator.Temp))
        {
            var transactionEvent = state.EntityManager.GetComponentData<TransactionEvents>(entity);
            if (transactionEvent.EventQueue.IsCreated)
            {
                transactionEvent.EventQueue.Dispose();
                transactionEvent.EventQueue = default;
                state.EntityManager.SetComponentData(entity, transactionEvent);
            }
        }
    }
}


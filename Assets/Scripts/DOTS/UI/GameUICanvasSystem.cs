using Assets.Scripts.DOTS.UI.UIPanels;
using Unity.Entities;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine;

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

public struct TransactionEventBuffer : IBufferElementData
{
    public TransactionEventType EventType;
}

public class OverlayPanels : IComponentData
{
    public StatsPanel statsPanel;
}

public class PanelControllers : IComponentData
{
    public PurchaseHousePanelController purchaseHousePanelController;
    public SpaceActionsPanelController spaceActionsPanelController;
    public BackdropController backdropController;
    public PurchasePropertyPanelController purchasePropertyPanelController;
    public PayRentPanelController payRentPanelController;
    public RollPanelController rollPanelController;
    public ChangeTurnPanelController changeTurnPanelController;
}

public struct RollPanelContext : IComponentData
{
    public int AmountRolled;
}

public class PopupManagers : IComponentData
{
    public PropertyPopupManager propertyPopupManager;
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
        state.RequireForUpdate<AudioSourceComponent>();
        state.RequireForUpdate<ClickSoundClipComponent>();
        state.RequireForUpdate<RollAmountComponent>();
        state.RequireForUpdate<RollEventBuffer>();
        state.RequireForUpdate<BuyHouseEventBuffer>();
        state.RequireForUpdate<TransactionEventBuffer>();
        state.RequireForUpdate<IsCurrentCharacterMoving>();

        state.EntityManager.CreateSingleton(new LastPropertyClicked { entity = Entity.Null });
        state.EntityManager.CreateSingleton(new OverlayPanels { statsPanel = null });
        state.EntityManager.CreateSingleton(new PanelControllers { purchaseHousePanelController = null, spaceActionsPanelController = null });
        state.EntityManager.CreateSingleton(new PopupManagers { propertyPopupManager = null});
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

        RollPanelContext rollPanelContext = new();
        ChangeTurnPanelContext changeTurnPanelContext = new();

        // TODO: Need a controller for the statsPanel
        StatsPanel statsPanel = new(topPanelRoot);
        RollPanel rollPanel = new(botPanelRoot);
        SpaceActionsPanel spaceActionsPanel = new(botPanelRoot);
        PurchaseHousePanel purchaseHousePanel = new(botPanelRoot, purchaseHousePanelContext);
        NoMonopolyYetPanel noMonopolyYetPanel = new(botPanelRoot);
        PurchasePropertyPanel purchasePropertyPanel = new(botPanelRoot);
        PayRentPanel payRentPanel = new(botPanelRoot);
        ChangeTurnPanel changeTurnPanel = new(botPanelRoot);

        var overlayPanels = SystemAPI.ManagedAPI.GetSingleton<OverlayPanels>();
        overlayPanels.statsPanel = statsPanel;

        // Loading Controllers
        var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();

        panelControllers.backdropController = new(backdrop);
        panelControllers.backdropController.RegisterPanelToHide(spaceActionsPanel.Panel);
        panelControllers.backdropController.RegisterPanelToHide(purchaseHousePanel.Panel);
        panelControllers.backdropController.RegisterPanelToHide(noMonopolyYetPanel.Panel);
        panelControllers.backdropController.RegisterPanelToHide(purchasePropertyPanel.Panel);
        panelControllers.purchaseHousePanelController = new(purchaseHousePanel);
        panelControllers.purchasePropertyPanelController = new(purchasePropertyPanel, purchasePropertyPanelContext);

        // -- Loading Audio --
        // -- PurchasePropertyPanel --
        var audioSourceRef = SystemAPI.ManagedAPI.GetSingleton<AudioSourceComponent>();
        var clickSound = SystemAPI.ManagedAPI.GetSingleton<ClickSoundClipComponent>().Value;
        if (audioSourceRef.AudioSourceGO != null)
        {
            var audioSourceGO = UnityEngine.Object.Instantiate(audioSourceRef.AudioSourceGO);
            var audioSource = audioSourceGO.GetComponent<AudioSource>();
            panelControllers.purchasePropertyPanelController.SetAudioSource(audioSource);
            panelControllers.purchasePropertyPanelController.SetClickSound(clickSound);
        }

        panelControllers.payRentPanelController = new(payRentPanel, payRentPanelContext);
        panelControllers.rollPanelController = new(rollPanel, rollPanelContext);
        panelControllers.changeTurnPanelController = new(changeTurnPanel, changeTurnPanelContext);
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

        // Button Actions
        var rollEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<RollEventBuffer>().Build();
        var transactionEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<TransactionEventBuffer>().Build();
        var buyHouseEventBufferQuery = SystemAPI.QueryBuilder().WithAllRW<BuyHouseEventBuffer>().Build();
        panelControllers.purchaseHousePanelController.SetEventBufferQuery(buyHouseEventBufferQuery);
        panelControllers.purchasePropertyPanelController.SetEventBufferQuery(transactionEventBufferQuery);
        panelControllers.payRentPanelController.SetEventBufferQuery(transactionEventBufferQuery);
        panelControllers.rollPanelController.SetEventBufferQuery(rollEventBufferQuery);
        panelControllers.changeTurnPanelController.SetEventBufferQuery(transactionEventBufferQuery);
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

        foreach (var rollAmount in SystemAPI.Query<RefRO<RollAmountComponent>>().WithChangeFilter<RollAmountComponent>())
        {
            RollPanelContext rollPanelContext = new(){ AmountRolled = rollAmount.ValueRO.AmountRolled };
            panelControllers.rollPanelController.Context = rollPanelContext;
            panelControllers.rollPanelController.Update();
        }

        foreach (var isCurrentCharacterMoving in 
                SystemAPI.Query<
                    RefRO<IsCurrentCharacterMoving>
                >()
                .WithChangeFilter<IsCurrentCharacterMoving>())
        {
            var isVisible = !isCurrentCharacterMoving.ValueRO.Value;
            ChangeTurnPanelContext changeTurnPanelContext = new(){ IsVisible = isVisible };
            panelControllers.changeTurnPanelController.Context = changeTurnPanelContext;
            panelControllers.changeTurnPanelController.UpdateVisibility();
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
                    panelControllers.rollPanelController.ShowPanel();
                    break;
                case GameState.Landing:
                    panelControllers.rollPanelController.HidePanel();
                    var spaceLanded = SystemAPI.GetSingleton<LandedOnSpace>();
                    if (SystemAPI.HasComponent<PropertySpaceTag>(spaceLanded.entity))
                    {
                        var popupManagers = SystemAPI.ManagedAPI.GetSingleton<PopupManagers>();
                        popupManagers.propertyPopupManager.TriggerPopup();
                    }
                    break;
            }
        }
    }

    public void OnStopRunning(ref SystemState state)
    {
        var panelsController = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
        panelsController.purchasePropertyPanelController.Dispose();
        panelsController.purchaseHousePanelController.Dispose();
        panelsController.spaceActionsPanelController.Dispose();
        panelsController.payRentPanelController.Dispose();
        panelsController.rollPanelController.Dispose();
        panelsController.backdropController.Dispose();
    }

    public void OnDestroy(ref SystemState state) { }
}


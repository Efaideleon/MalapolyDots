using DOTS.Characters;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.Mediator;
using DOTS.UI.Controllers;
using DOTS.UI.Panels;
using DOTS.UI.Systems;
using Unity.Entities;
using UnityEngine.InputSystem;

public partial struct GamePlayUIMediator : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MoneyComponent>();
        state.RequireForUpdate<PanelControllers>();
        state.RequireForUpdate<PopupManagers>();
        state.RequireForUpdate<ClickData>();
        state.RequireForUpdate<PurchasePropertyPanelContextComponent>();
        state.RequireForUpdate<PurhcaseHousePanelContextComponent>();
        state.RequireForUpdate<LastPropertyClicked>();
        state.RequireForUpdate<IsCurrentCharacterMoving>();
        state.RequireForUpdate<PanelControllers>();
        state.RequireForUpdate<CurrentPlayerID>();
        state.EntityManager.CreateSingleton(new LastPropertyClicked { entity = Entity.Null });
    }
    
    public void OnUpdate(ref SystemState state)
    {

        foreach (var isCurrentCharacterMoving in 
                SystemAPI.Query<
                RefRO<IsCurrentCharacterMoving>
                >()
                .WithChangeFilter<IsCurrentCharacterMoving>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                var isVisible = !isCurrentCharacterMoving.ValueRO.Value;
                ChangeTurnPanelContext changeTurnPanelContext = new(){ IsVisible = isVisible };
                if (panelControllers.changeTurnPanelController != null)
                {
                    panelControllers.changeTurnPanelController.Context = changeTurnPanelContext;
                    panelControllers.changeTurnPanelController.UpdateVisibility();
                }
            }
        }

        foreach (var spaceActionsContext in
                SystemAPI.Query<
                RefRO<SpaceActionsPanelContextComponent>
                >()
                .WithChangeFilter<SpaceActionsPanelContextComponent>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                if (panelControllers.spaceActionsPanelController != null)
                {
                    panelControllers.spaceActionsPanelController.Context = spaceActionsContext.ValueRO.Value;
                }
            }
        }

        foreach (var purchaseHousePanelContext in
                SystemAPI.Query<
                RefRO<PurhcaseHousePanelContextComponent>
                >()
                .WithChangeFilter<PurhcaseHousePanelContextComponent>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                if (panelControllers.purchaseHousePanelController != null)
                {
                    panelControllers.purchaseHousePanelController.PurchaseHousePanel.Context = purchaseHousePanelContext.ValueRO.Value;
                    panelControllers.purchaseHousePanelController.PurchaseHousePanel.Update();
                }
            }
        }

        foreach (var purchasePropertyPanelContext in
                SystemAPI.Query<
                RefRO<PurchasePropertyPanelContextComponent>
                >()
                .WithChangeFilter<PurchasePropertyPanelContextComponent>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            var spriteRegistry = SystemAPI.ManagedAPI.GetSingleton<SpriteRegistryComponent>();
            if (panelControllers != null && spriteRegistry.Value != null)
            {
                if (panelControllers.purchasePropertyPanelController != null)
                {
                    // TODO: not consistent with the PurhcaseHousePanel.
                    // Here we assigned the Context to the controller instead of the panel itself
                    var context = purchasePropertyPanelContext.ValueRO.Value;
                    spriteRegistry.Value.TryGetValue(context.Name, out var sprite); 
                    panelControllers.purchasePropertyPanelController.Context = context;
                    panelControllers.purchasePropertyPanelController.ManagedContext.sprite = sprite;
                    panelControllers.purchasePropertyPanelController.Update();
                }
            }
        }

        foreach (var payRentPanelContext in SystemAPI.Query<
                RefRO<PayRentPanelContextComponent>
                >()
                .WithChangeFilter<PayRentPanelContextComponent>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                if (panelControllers.payRentPanelController != null)
                {
                    panelControllers.payRentPanelController.Context = payRentPanelContext.ValueRO.Value;
                    panelControllers.payRentPanelController.Update();
                }
            }
        }

        foreach (var rollAmount in SystemAPI.Query<RefRO<RollAmountCountDown>>().WithChangeFilter<RollAmountCountDown>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                if (panelControllers.rollPanelController != null)
                {
                    RollPanelContext rollPanelContext = new(){ AmountRolled = rollAmount.ValueRO.Value };
                    panelControllers.rollPanelController.Context = rollPanelContext;
                    panelControllers.rollPanelController.Update();
                }
            }
        }

        // When an entity is clicked show the actions panel 
        foreach (var clickedProperty in
                SystemAPI.Query<
                RefRW<ClickedPropertyComponent>
                >()
                .WithChangeFilter<ClickedPropertyComponent>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                if (panelControllers.spaceActionsPanelController != null)
                {
                    if (clickedProperty.ValueRO.entity != Entity.Null)
                    {
                        var clickData = SystemAPI.GetSingleton<ClickData>();
                        var lastPropertyClicked = SystemAPI.GetSingletonRW<LastPropertyClicked>();
                        lastPropertyClicked.ValueRW.entity = clickedProperty.ValueRO.entity;

                        switch (clickData.Phase)
                        {
                            case InputActionPhase.Canceled:
                                panelControllers.spaceActionsPanelController.SpaceActionsPanel.Show();
                                panelControllers.backdropController.ShowBackdropWithDelay();
                                break;
                        }
                        // TODO: The backdrop panel should appear whenever one of the hideable panels is appears.
                        var clickedPropertyComp = SystemAPI.GetSingletonRW<ClickedPropertyComponent>();
                        clickedPropertyComp.ValueRW.entity = Entity.Null;
                    }
                }
            }
        }

        foreach (var gameState in SystemAPI.Query<RefRO<GameStateComponent>>().WithChangeFilter<GameStateComponent>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                if (panelControllers.rollPanelController != null)
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
        }

        // Updates the context for the popup when the player lands on a property space.
        foreach (var landOnProperty in SystemAPI.Query<RefRO<LandedOnSpace>>().WithChangeFilter<LandedOnSpace>())
        {
            var landOnPropertyEntity = landOnProperty.ValueRO.entity;
            if (landOnPropertyEntity != null && SystemAPI.HasComponent<PropertySpaceTag>(landOnPropertyEntity))
            {
                var popupManagers = SystemAPI.ManagedAPI.GetSingleton<PopupManagers>();

                if (popupManagers != null)
                {
                    if (popupManagers.propertyPopupManager != null)
                    {
                        PropertyPopupManagerContext propertyPopupManagerContext = new ()
                        {
                            OwnerID = SystemAPI.GetComponent<OwnerComponent>(landOnPropertyEntity).ID,
                            CurrentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>().Value
                        };
                        popupManagers.propertyPopupManager.Context = propertyPopupManagerContext;
                    }
                }
            }
        }
    }
}

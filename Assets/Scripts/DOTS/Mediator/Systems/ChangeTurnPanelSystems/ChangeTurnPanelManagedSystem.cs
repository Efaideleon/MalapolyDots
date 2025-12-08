using DOTS.Characters;
using DOTS.GamePlay;
using DOTS.UI.Controllers;
using DOTS.UI.Mediator.Systems.RollPanelSystems;
using Unity.Entities;

public partial struct ChangeTurnPanelManagedSystem : ISystem
{
    public ComponentLookup<PlayerMovementState> playerMovementStateLookup;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PanelControllers>();
        state.RequireForUpdate<RollPanelVisibleState>();
        state.RequireForUpdate<CurrentActivePlayer>();

        playerMovementStateLookup = SystemAPI.GetComponentLookup<PlayerMovementState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        playerMovementStateLookup.Update(ref state);

        var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;

        if (playerMovementStateLookup.HasComponent(activePlayerEntity))
        {
            if (playerMovementStateLookup.DidChange(activePlayerEntity, state.LastSystemVersion))
            {
                var playerMoveState = playerMovementStateLookup[activePlayerEntity];
                PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelControllers != null)
                {
                    var isRollVisible = SystemAPI.GetSingleton<RollPanelVisibleState>().Value;
                    var isVisible = !(playerMoveState.Value == MoveState.Walking) && !isRollVisible;
                    ChangeTurnPanelContext changeTurnPanelContext = new() { IsVisible = isVisible };
                    if (panelControllers.changeTurnPanelController != null)
                    {
                        panelControllers.changeTurnPanelController.Context = changeTurnPanelContext;
                        panelControllers.changeTurnPanelController.UpdateVisibility();
                    }
                }
            }
        }

        foreach (var isRollVisible in SystemAPI.Query<RefRO<RollPanelVisibleState>>().WithChangeFilter<RollPanelVisibleState>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                var isVisible = !isRollVisible.ValueRO.Value;
                ChangeTurnPanelContext changeTurnPanelContext = new() { IsVisible = isVisible };
                if (panelControllers.changeTurnPanelController != null)
                {
                    panelControllers.changeTurnPanelController.Context = changeTurnPanelContext;
                    panelControllers.changeTurnPanelController.UpdateVisibility();
                }
            }
        }
    }
}

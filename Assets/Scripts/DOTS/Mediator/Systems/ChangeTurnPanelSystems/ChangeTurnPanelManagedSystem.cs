using DOTS.Characters;
using DOTS.GamePlay;
using DOTS.UI.Controllers;
using DOTS.UI.Mediator.Systems.RollPanelSystems;
using Unity.Entities;

public partial struct ChangeTurnPanelManagedSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PanelControllers>();
        state.RequireForUpdate<RollPanelVisibleState>();
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerMoveState, _) in 
                SystemAPI.Query<
                RefRO<PlayerMovementState>,
                RefRO<ActivePlayer>
                >()
                .WithChangeFilter<PlayerMovementState>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                var isRollVisible = SystemAPI.GetSingleton<RollPanelVisibleState>().Value;
                var isVisible = !(playerMoveState.ValueRO.Value == MoveState.Walking) && !isRollVisible;
                ChangeTurnPanelContext changeTurnPanelContext = new(){ IsVisible = isVisible };
                if (panelControllers.changeTurnPanelController != null)
                {
                    panelControllers.changeTurnPanelController.Context = changeTurnPanelContext;
                    panelControllers.changeTurnPanelController.UpdateVisibility();
                }
            }
        }

        foreach (var isRollVisible in SystemAPI.Query<RefRO<RollPanelVisibleState>>().WithChangeFilter<RollPanelVisibleState>())
        {
            PanelControllers panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
            if (panelControllers != null)
            {
                var isVisible = !isRollVisible.ValueRO.Value;
                ChangeTurnPanelContext changeTurnPanelContext = new(){ IsVisible = isVisible };
                if (panelControllers.changeTurnPanelController != null)
                {
                    panelControllers.changeTurnPanelController.Context = changeTurnPanelContext;
                    panelControllers.changeTurnPanelController.UpdateVisibility();
                }
            }
        }
    }
}

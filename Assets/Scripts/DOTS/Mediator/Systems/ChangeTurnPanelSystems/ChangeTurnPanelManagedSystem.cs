using DOTS.GamePlay;
using DOTS.UI.Controllers;
using DOTS.UI.Mediator.Systems.RollPanelSystems;
using Unity.Entities;

public partial struct ChangeTurnPanelManagedSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<IsCurrentCharacterMoving>();
        state.RequireForUpdate<PanelControllers>();
        state.RequireForUpdate<RollPanelVisibleState>();
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
                var isRollVisible = SystemAPI.GetSingleton<RollPanelVisibleState>().Value;
                var isVisible = !isCurrentCharacterMoving.ValueRO.Value && !isRollVisible;
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

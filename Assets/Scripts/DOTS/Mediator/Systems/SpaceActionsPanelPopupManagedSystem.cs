using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;
using UnityEngine.InputSystem;

public partial struct SpaceActionsPanelPopupManagedSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ShowActionsPanelBuffer>();
        state.RequireForUpdate<PanelControllers>();
        state.RequireForUpdate<ClickedPropertyComponent>();
        state.RequireForUpdate<ClickData>();
        state.RequireForUpdate<LastPropertyClicked>();
    }
    public void OnUpdate(ref SystemState state)
    {
        foreach (var buffer in SystemAPI.Query<DynamicBuffer<ShowActionsPanelBuffer>>().WithChangeFilter<ShowActionsPanelBuffer>())
        {
            foreach (var e in buffer)
            {
                var panelControllers = SystemAPI.ManagedAPI.GetSingleton<PanelControllers>();
                if (panelControllers == null)
                    break;
                if (panelControllers.spaceActionsPanelController == null)
                    break;
                if (panelControllers.backdropController == null)
                    break;

                panelControllers.spaceActionsPanelController.SpaceActionsPanel.Show();
                panelControllers.backdropController.ShowBackdrop();
            }
            buffer.Clear();
        }

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
                                panelControllers.backdropController.ShowBackdrop();
                                break;
                        }
                        // TODO: The backdrop panel should appear whenever one of the hideable panels is appears.
                        //var clickedPropertyComp = SystemAPI.GetSingletonRW<ClickedPropertyComponent>();
                        //clickedPropertyComp.ValueRW.entity = Entity.Null;
                    }
                }
            }
        }
    }
}

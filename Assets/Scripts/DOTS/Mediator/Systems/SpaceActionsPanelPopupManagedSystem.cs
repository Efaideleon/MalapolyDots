using DOTS.DataComponents;
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

                UnityEngine.Debug.Log($"[SpaceActionsPanelPopupManagedSystem] | Show space actions panels");
                panelControllers.spaceActionsPanelController.ShowPanel();
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
                        var clickPhase = clickedProperty.ValueRO.ClickPhase;
                        UnityEngine.Debug.Log($"[SpaceActionsPanelPopupManagedSystem] | clickData.Phase {clickPhase}");
                        var lastPropertyClicked = SystemAPI.GetSingletonRW<LastPropertyClicked>();

                        if (SystemAPI.HasComponent<NameComponent>(clickedProperty.ValueRO.entity))
                        {
                            var name = SystemAPI.GetComponent<NameComponent>(clickedProperty.ValueRO.entity);
                            UnityEngine.Debug.Log($"[SpaceActionsPanelPopupManagedSystem] | Entity Hit: {name.Value}");
                        }
                        lastPropertyClicked.ValueRW.entity = clickedProperty.ValueRO.entity;

                        switch (clickPhase)
                        {
                            case InputActionPhase.Canceled:
                                panelControllers.spaceActionsPanelController.ShowPanel();
                                panelControllers.backdropController.ShowBackdrop();
                                break;
                        }
                    }
                }
            }
        }
    }
}

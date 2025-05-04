using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.UI.Controllers;
using DOTS.UI.Panels;
using Unity.Entities;

namespace DOTS.Mediator.Systems
{
    public partial struct PropertyPopupManagerTriggerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<PanelControllers>();
            state.RequireForUpdate<PropertySpaceTag>();
            state.RequireForUpdate<LandedOnSpace>();
            state.RequireForUpdate<PopupManagers>();
        }

        public void OnUpdate(ref SystemState state)
        {
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
                                    PropertyPopupManagerContext propertyPopupManagerContext = new ()
                                    {
                                        OwnerID = SystemAPI.GetComponent<OwnerComponent>(spaceLanded.entity).ID,
                                        CurrentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>().Value
                                    };
                                    popupManagers.propertyPopupManager.Context = propertyPopupManagerContext;
                                    popupManagers.propertyPopupManager.TriggerPopup();
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}

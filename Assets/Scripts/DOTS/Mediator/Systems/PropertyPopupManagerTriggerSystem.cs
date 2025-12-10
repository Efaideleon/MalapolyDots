using DOTS.Characters;
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
        public ComponentLookup<GameStateComponent> gameStateLookup;
        public ComponentLookup<SpaceLandedOn> spaceLandedOnLookup;
        public ComponentLookup<PropertySpaceTag> propertySpaceLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<PropertySpaceTag>();
            state.RequireForUpdate<LandedOnSpace>();
            state.RequireForUpdate<PopupManagers>();
            state.RequireForUpdate<CurrentActivePlayer>();

            gameStateLookup = SystemAPI.GetComponentLookup<GameStateComponent>();
            propertySpaceLookup = SystemAPI.GetComponentLookup<PropertySpaceTag>();
            spaceLandedOnLookup = SystemAPI.GetComponentLookup<SpaceLandedOn>();
        }

        public void OnUpdate(ref SystemState state)
        {
            gameStateLookup.Update(ref state);
            propertySpaceLookup.Update(ref state);
            spaceLandedOnLookup.Update(ref state);

            Entity gameStateEntity = SystemAPI.GetSingletonEntity<GameStateComponent>();
            Entity activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;

            if (!gameStateLookup.HasComponent(gameStateEntity)) return;
            if (!gameStateLookup.DidChange(gameStateEntity, state.LastSystemVersion)) return;
            if (!spaceLandedOnLookup.HasComponent(activePlayerEntity)) return;

            Entity spaceLanded = spaceLandedOnLookup[activePlayerEntity].entity;
            if (propertySpaceLookup.HasComponent(spaceLanded)) return;

            GameState gameState = gameStateLookup[gameStateEntity].State;

            var popupManagers = SystemAPI.ManagedAPI.GetSingleton<PopupManagers>();
            if (popupManagers != null)
            {
                if (popupManagers.propertyPopupManager != null)
                {
                    switch (gameState)
                    {
                        case GameState.Landing:
                            PropertyPopupManagerContext propertyPopupManagerContext = new()
                            {
                                OwnerID = SystemAPI.GetComponent<OwnerComponent>(spaceLanded).ID,
                                CurrentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>().Value
                            };
                            popupManagers.propertyPopupManager.Context = propertyPopupManagerContext;
                            popupManagers.propertyPopupManager.TriggerPopup();
                            break;
                    }
                }
            }
        }
    }
}

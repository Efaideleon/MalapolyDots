using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.UI.Controllers;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.Mediator.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct PropertyPopupManagerTriggerSystem : ISystem
    {
        private uint lastPresentedLanding;
        public ComponentLookup<GameStateComponent> gameStateLookup;
        public ComponentLookup<SpaceLandedOn> spaceLandedOnLookup;
        public ComponentLookup<PropertySpaceTag> propertySpaceLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<PropertySpaceTag>();
            state.RequireForUpdate<PopupManagers>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GhostDataLoadedTag>();

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
            var landing = gameStateLookup[gameStateEntity];
            if (!landing.HasUnseenLanding(activePlayerEntity, lastPresentedLanding)) return;
            if (!spaceLandedOnLookup.HasComponent(activePlayerEntity)) return;

            Entity spaceLanded = spaceLandedOnLookup[activePlayerEntity].entity;
            if (spaceLanded != landing.LandingSpace || !propertySpaceLookup.HasComponent(spaceLanded)) return;

            GameState gameState = gameStateLookup[gameStateEntity].State;
            UnityEngine.Debug.Log($"[PropertyPopupManagerTriggerSystem] | running..");

            var popupManagers = SystemAPI.ManagedAPI.GetSingleton<PopupManagers>();
            if (popupManagers != null)
            {
                if (popupManagers.propertyPopupManager != null)
                {
                    switch (gameState)
                    {
                        case GameState.Landing:
                            // if is not the active player, don't show this popup.
                            var currentActivePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;

                            var clientId = SystemAPI.GetSingleton<NetworkId>();
                            var playerId = SystemAPI.GetComponent<GhostOwner>(currentActivePlayer);
                            bool isLocalPlayer = clientId.Value == playerId.NetworkId;

                            if (!isLocalPlayer) return;
                            lastPresentedLanding = landing.LandingSequence;

                            PropertyPopupManagerContext propertyPopupManagerContext = new()
                            {
                                OwnerID = SystemAPI.GetComponent<OwnerComponent>(spaceLanded).ID,
                                CurrentPlayerID = playerId.NetworkId,
                                isLocal = isLocalPlayer
                            };

                            popupManagers.propertyPopupManager.Context = propertyPopupManagerContext;
                            UnityEngine.Debug.Log($"[PropertyPopupManagerTriggerSystem] | showing property popup");

                            popupManagers.propertyPopupManager.TriggerPopup();
                            break;
                    }
                }
            }
        }
    }
}

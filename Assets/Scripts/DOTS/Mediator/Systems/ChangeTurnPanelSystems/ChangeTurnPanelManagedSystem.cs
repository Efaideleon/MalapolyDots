using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.UI.Controllers;
using DOTS.UI.Controllers;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.Mediator.Systems.ChangeTurnPanelSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ChangeTurnPanelManagedSystem : ISystem
    {
        public ComponentLookup<PlayerMovementState> playerMovementStateLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PanelControllerService>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GhostDataLoadedTag>();

            playerMovementStateLookup = SystemAPI.GetComponentLookup<PlayerMovementState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            playerMovementStateLookup.Update(ref state);

            var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            if (activePlayerEntity == Entity.Null)
            {
                return;
            }

            var service = SystemAPI.ManagedAPI.GetSingleton<PanelControllerService>();
            var hasTurnPanel = service.TryGet<ChangeTurnPanelController>(out var turnPanel);
            var hasRollPanel = service.TryGet<RollPanelController>(out var rollPanel);
            if (!hasRollPanel || !hasTurnPanel)
            {
                return;
            }
            var clientId = SystemAPI.GetSingleton<NetworkId>();
            var playerId = SystemAPI.GetComponent<GhostOwner>(activePlayerEntity);
            bool isLocalPlayer = clientId.Value == playerId.NetworkId;

            var playerMoveState = playerMovementStateLookup[activePlayerEntity];

            var isRollVisible = rollPanel.IsVisible;
            var shouldTurnBeVisible = !(playerMoveState.Value == MoveState.Walking) && !isRollVisible && isLocalPlayer;

            if (shouldTurnBeVisible && turnPanel.IsVisible)
            {
                return;
            }

            if (!shouldTurnBeVisible && turnPanel.IsHiding)
            {
                return;
            }

            UnityEngine.Debug.Log($"[ChangeTurnPanelManagedSystem] | shouldTurnBeVisible: {shouldTurnBeVisible}");
            ChangeTurnPanelContext changeTurnPanelContext = new() { IsVisible = shouldTurnBeVisible };
            turnPanel.Context = changeTurnPanelContext;
            turnPanel.UpdateVisibility();
        }
    }
}

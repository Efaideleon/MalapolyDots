using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.UI.Controllers;
using DOTS.GamePlay;
using DOTS.UI.Controllers;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.Mediator.Systems.RollPanelSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct RollPanelPopupManagedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PanelControllerService>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var currentActivePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>();
            if (currentActivePlayer.Entity == default)
                return;

            var service = SystemAPI.ManagedAPI.GetSingleton<PanelControllerService>();
            if (!service.TryGet<RollPanelController>(out var rollPanel))
                return;

            var clientId = SystemAPI.GetSingleton<NetworkId>();
            var playerId = SystemAPI.GetComponent<GhostOwner>(currentActivePlayer.Entity);
            var gameState = SystemAPI.GetSingleton<GameStateComponent>();
            bool isLocalPlayer = clientId.Value == playerId.NetworkId;
            var newPanelState = GetPanelState(gameState.State, isLocalPlayer);

            rollPanel.SetState(newPanelState);
        }

        private readonly RollPanelState GetPanelState(GameState gameState, bool isLocalPlayer)
        {
            if (!isLocalPlayer)
            {
                return RollPanelState.CountingDown;
            }

            if (gameState == GameState.Rolling)
            {
                return RollPanelState.Ready;
            }

            if (gameState == GameState.Walking)
            {
                return RollPanelState.CountingDown;
            }

            return RollPanelState.Hidden;
        }
    }
}

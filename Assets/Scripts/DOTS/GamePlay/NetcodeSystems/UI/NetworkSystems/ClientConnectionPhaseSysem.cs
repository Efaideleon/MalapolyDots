using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ClientConnectionPhaseSystem : ISystem
    {
        private ComponentLookup<GameMenuPhaseComponent> gameMenuPhaseLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<NetworkStreamConnection>();
            state.RequireForUpdate<GameMenuPhaseComponent>();
            gameMenuPhaseLookup = SystemAPI.GetComponentLookup<GameMenuPhaseComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            gameMenuPhaseLookup.Update(ref state);
            var gameMenuPhaseEntity = SystemAPI.GetSingletonEntity<GameMenuPhaseComponent>();

            // If we are in the HostSetup or the JoinSetup, then go to the lobby.
            var currentMenuPhase = gameMenuPhaseLookup[gameMenuPhaseEntity].Value;
            if (currentMenuPhase == GameMenuPhase.HostSetup || currentMenuPhase == GameMenuPhase.JoinSetup)
            {
                // If we are already in lobby, return;
                if(currentMenuPhase == GameMenuPhase.Lobby)
                    return;

                gameMenuPhaseLookup.GetRefRW(gameMenuPhaseEntity).ValueRW.Value = GameMenuPhase.Lobby;
                UnityEngine.Debug.Log($"[ClientConnectionPhaseSystem] | Client Connected -> Lobby");
            }
        }
    }
}

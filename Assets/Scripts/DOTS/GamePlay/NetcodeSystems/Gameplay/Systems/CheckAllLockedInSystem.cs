using Assets.Common;
using Assets.Scripts.DOTS.DataComponents;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems;
using DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems;
using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(LockinCharacterSystem))]
    [UpdateAfter(typeof(CreatePlayerConnectionDataEntity))]
    public partial struct CheckAllLockedInSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerConnectionData>();
            state.RequireForUpdate<GamePhaseGhostComponent>();
            state.RequireForUpdate<LobbyStartState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.GetSingleton<LobbyStartState>().Started ||
                SystemAPI.GetSingleton<GamePhaseGhostComponent>().GamePhase == GamePhase.Game) return;
            int players = 0;
            foreach (var player in SystemAPI.Query<RefRO<PlayerConnectionData>>())
            {
                players++;
                if (!player.ValueRO.IsLockedIn || player.ValueRO.CharacterSelected == CharactersEnum.Default) return;
            }
            if (players < 1 || players > 6 || players != NetworkRequests.ExpectedLobbyPlayers) return;
            if (SoloSession.Active)
            {
                foreach (var player in SystemAPI.Query<RefRO<PlayerConnectionData>>())
                    if (!global::DOTS.GamePlay.SoloOpponentSetup.Prepare(state.EntityManager, player.ValueRO.OwnerNetworkId, player.ValueRO.CharacterSelected)) return;
            }
            if (!SystemAPI.HasSingleton<GameMenuToGameSceneTag>())
                SystemAPI.GetSingletonRW<GamePhaseGhostComponent>().ValueRW.GamePhase = GamePhase.Game;
        }
    }

    public struct GameMenuToGameSceneTag : IComponentData { }
}

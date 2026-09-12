using Assets.Common;
using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    public struct LobbyStartState : IComponentData { public bool Started; }
    public struct CharacterSelectionNotified : IComponentData { }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct AllGoToCharacterSelectClientSystem : ISystem
    {
        public void OnCreate(ref SystemState state) { state.RequireForUpdate<GameMenuPhaseComponent>(); }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (_, rpcEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoToCharacterSelectRpc>().WithEntityAccess())
            {
                SystemAPI.GetSingletonRW<GameMenuPhaseComponent>().ValueRW.Value = GameMenuPhase.CharacterSelect;
                ecb.DestroyEntity(rpcEntity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GoToCharacterSelectServerSystem : ISystem
    {
        private int m_LobbyVersion;
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new LobbyStartState());
            m_LobbyVersion = NetworkRequests.LobbyVersion;
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            if (m_LobbyVersion != NetworkRequests.LobbyVersion)
            {
                m_LobbyVersion = NetworkRequests.LobbyVersion;
                SystemAPI.SetSingleton(new LobbyStartState());
                foreach (var (_, connection) in SystemAPI.Query<RefRO<CharacterSelectionNotified>>().WithEntityAccess())
                    ecb.RemoveComponent<CharacterSelectionNotified>(connection);
            }
            // Start is a local host action, set only after the host locks the online session.
            // Remote clients cannot request this transition with an RPC.
            foreach (var (_, rpc) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoToCharacterSelectRpc>().WithEntityAccess())
                ecb.DestroyEntity(rpc);

            if (NetworkRequests.StartGame)
            {
                NetworkRequests.StartGame = false;
                SystemAPI.GetSingletonRW<LobbyStartState>().ValueRW.Started = true;
            }
            if (SystemAPI.GetSingleton<LobbyStartState>().Started)
            {
                // Notify each connected participant once, including connections still completing at Start.
                foreach (var (_, connection) in SystemAPI.Query<RefRO<NetworkId>>().WithAll<NetworkStreamInGame>().WithNone<CharacterSelectionNotified>().WithEntityAccess())
                {
                    var rpc = ecb.CreateEntity();
                    ecb.AddComponent<GoToCharacterSelectRpc>(rpc);
                    ecb.AddComponent(rpc, new SendRpcCommandRequest { TargetConnection = connection });
                    ecb.AddComponent<CharacterSelectionNotified>(connection);
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct GoToCharacterSelectRpc : IRpcCommand { }
}

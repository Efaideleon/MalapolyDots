using DOTS.DataComponents;
using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct GameMenuExitConnectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<ExitConnectionClickEvent>();
            state.RequireForUpdate<NetworkRoleTypeComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // TODO: This should only run when the host exits the connection.
            // TODO: if the client clicks on the exit button only terminate its own connection and send back to main menu.
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            var networkRole = SystemAPI.GetSingleton<NetworkRoleTypeComponent>();

            switch (networkRole.Value)
            {
                case NetworkRole.Host:
                    var hostRpcEntity = ecb.CreateEntity();
                    ecb.AddComponent<SendRpcCommandRequest>(hostRpcEntity);
                    ecb.AddComponent<TerminateAllClientsConnectionRpc>(hostRpcEntity);
                    break;
                case NetworkRole.Client:
                    var clientRpcEntity = ecb.CreateEntity();
                    ecb.AddComponent<SendRpcCommandRequest>(clientRpcEntity);
                    ecb.AddComponent<TerminateConnectionRpc>(clientRpcEntity);
                    break;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct TerminateAllClientsConnectionRpc : IRpcCommand
    {}
    
    public struct TerminateConnectionRpc : IRpcCommand
    {}
}

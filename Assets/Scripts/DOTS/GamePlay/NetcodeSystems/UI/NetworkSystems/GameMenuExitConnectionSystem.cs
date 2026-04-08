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
        }

        public void OnUpdate(ref SystemState state)
        {
            // TODO: This should only run when the host exits the connection.
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            var rpcEntity = ecb.CreateEntity();
            ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
            ecb.AddComponent<TerminateAllClientsConnectionRpc>(rpcEntity);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct TerminateAllClientsConnectionRpc : IRpcCommand
    {}
}

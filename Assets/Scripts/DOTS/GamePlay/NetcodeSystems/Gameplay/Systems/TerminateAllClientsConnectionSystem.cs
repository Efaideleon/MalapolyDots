using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems;
using TitleScreen.NetworkUI.Components;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct TerminateAllClientsConnectionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (_, _, rpcEntity) in SystemAPI.Query
                <
                    RefRO<ReceiveRpcCommandRequest>,
                    RefRO<TerminateAllClientsConnectionRpc>
                >() .WithEntityAccess())
            {
                var disconnectEntity = ecb.CreateEntity();
                ecb.AddComponent<SendRpcCommandRequest>(disconnectEntity);
                ecb.AddComponent<GoToMainMenuRpc>(disconnectEntity);

                foreach (var (n, connection) in SystemAPI.Query<NetworkId>().WithEntityAccess())
                {
                    ecb.AddComponent(connection, new NetworkStreamRequestDisconnect { Reason = NetworkStreamDisconnectReason.ConnectionClose });
                    UnityEngine.Debug.Log($"[TerminateAllClientConnectionSystem] | ExitConnection pressed.");
                    UnityEngine.Debug.Log($"[TerminateAllClientConnectionSystem] | connection to terminate: {n.Value}");
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
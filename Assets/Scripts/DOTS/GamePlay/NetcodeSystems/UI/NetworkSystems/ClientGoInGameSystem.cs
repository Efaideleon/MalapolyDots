using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ClientGoInGameSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (networkId, conn, entity) in SystemAPI.Query<RefRO<NetworkId>, RefRO<NetworkStreamConnection>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
            {
                ecb.AddComponent<NetworkStreamInGame>(entity);
                UnityEngine.Debug.Log($"[ClientGoInGameSystem] | Client entered Game.");

                var entityRpc = ecb.CreateEntity();
                ecb.AddComponent<ClientGoInGameRequestRpc>(entityRpc);
                ecb.AddComponent<SendRpcCommandRequest>(entityRpc);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct ClientGoInGameRequestRpc : IRpcCommand
    { }
}

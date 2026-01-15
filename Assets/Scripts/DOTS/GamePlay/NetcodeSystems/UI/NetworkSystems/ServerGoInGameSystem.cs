using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ServerGoInGameSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (request, rpcSource, reqEntity) in SystemAPI.Query<RefRO<ClientGoInGameRequestRpc>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                var connectionEntity = rpcSource.ValueRO.SourceConnection;

                if (state.EntityManager.HasComponent<NetworkStreamConnection>(connectionEntity) && !state.EntityManager.HasComponent<NetworkStreamInGame>(connectionEntity))
                {
                    ecb.AddComponent<NetworkStreamInGame>(connectionEntity);
                }
                ecb.DestroyEntity(reqEntity);
                UnityEngine.Debug.Log($"[ServerGoInGameSystem] | Server entered Game.");
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

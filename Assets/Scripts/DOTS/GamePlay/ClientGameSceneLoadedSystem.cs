using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ClientGameSceneLoadedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (_, req, rpcEntity) in SystemAPI.Query<RefRO<ClientGameSceneLoadedRPC>, RefRO<ReceiveRpcCommandRequest>>().WithEntityAccess())
            {
                ecb.AddComponent<ClientGameSceneLoadedTag>(req.ValueRO.SourceConnection);
                UnityEngine.Debug.Log($"[ClientGameSceneLoadedSystem] | Client has loaded scene");

                ecb.DestroyEntity(rpcEntity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct ClientGameSceneLoadedTag : IComponentData
    { }
}



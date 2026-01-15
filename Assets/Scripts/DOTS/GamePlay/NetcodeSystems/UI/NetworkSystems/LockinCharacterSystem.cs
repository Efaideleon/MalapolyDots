using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct LockinCharacterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<PlayerConnectionData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (req, reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAny<LockInCharacterEvent>().WithEntityAccess())
            {
                var networkId = SystemAPI.GetComponent<NetworkId>(req.ValueRO.SourceConnection);
                foreach (var playerConnectionData in SystemAPI.Query<RefRW<PlayerConnectionData>>())
                {
                    if (networkId.Value == playerConnectionData.ValueRO.Owner)
                    {
                        playerConnectionData.ValueRW.IsLockedIn = true;
                    }
                }
                ecb.DestroyEntity(reqEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}


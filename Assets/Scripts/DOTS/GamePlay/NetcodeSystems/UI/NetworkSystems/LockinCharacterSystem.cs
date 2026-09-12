using Assets.Scripts.DOTS.DataComponents;
using DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(PrepickCharacterSystem))]
    public partial struct LockinCharacterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<PlayerConnectionData>();
            state.RequireForUpdate<LobbyStartState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (request, rpc) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<LockInCharacterEvent>().WithEntityAccess())
            {
                ecb.DestroyEntity(rpc);
                if (!SystemAPI.GetSingleton<LobbyStartState>().Started ||
                    !SystemAPI.HasComponent<NetworkId>(request.ValueRO.SourceConnection)) continue;
                int id = SystemAPI.GetComponent<NetworkId>(request.ValueRO.SourceConnection).Value;
                foreach (var player in SystemAPI.Query<RefRW<PlayerConnectionData>>())
                {
                    if (player.ValueRO.OwnerNetworkId != id || player.ValueRO.IsLockedIn ||
                        player.ValueRO.CharacterSelected == CharactersEnum.Default) continue;
                    bool taken = false;
                    foreach (var other in SystemAPI.Query<RefRO<PlayerConnectionData>>())
                        if (other.ValueRO.OwnerNetworkId != id && other.ValueRO.IsLockedIn &&
                            other.ValueRO.CharacterSelected == player.ValueRO.CharacterSelected) taken = true;
                    if (!taken) player.ValueRW.IsLockedIn = true;
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

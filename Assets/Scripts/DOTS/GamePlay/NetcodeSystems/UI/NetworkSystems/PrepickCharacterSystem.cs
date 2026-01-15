using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct PrepickCharacterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<PlayerConnectionData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (req, prepickCharacter, reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<PrepickCharacter>>().WithEntityAccess())
            {
                foreach (var playerConnectionData in SystemAPI.Query<RefRW<PlayerConnectionData>>())
                {
                    var networkId = SystemAPI.GetComponent<NetworkId>(req.ValueRO.SourceConnection);
                    if (playerConnectionData.ValueRO.Owner == networkId.Value && !playerConnectionData.ValueRO.IsLockedIn)
                    {
                        playerConnectionData.ValueRW.CharacterSelected = prepickCharacter.ValueRO.Character;
                    }
                }

                ecb.DestroyEntity(reqEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}


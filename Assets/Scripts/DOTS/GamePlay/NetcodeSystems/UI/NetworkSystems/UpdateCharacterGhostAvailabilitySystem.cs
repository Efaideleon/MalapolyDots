using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.Authorings;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct UpdateCharacterGhostAvailabilitySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<PlayerConnectionData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var playerConnectionData in SystemAPI.Query<RefRO<PlayerConnectionData>>().WithChangeFilter<PlayerConnectionData>())
            {
                foreach (var prepickedCharacter in SystemAPI.Query<RefRW<PrepickedCharacter>>())
                {
                    // if the player previously owned this character, but picked a new one, then free the previous character.
                    if (playerConnectionData.ValueRO.OwnerNetworkId == prepickedCharacter.ValueRO.OwnerNetworkId && playerConnectionData.ValueRO.CharacterSelected != prepickedCharacter.ValueRO.Character)
                    {
                        prepickedCharacter.ValueRW.OwnerNetworkId = default;
                        prepickedCharacter.ValueRW.PrePicked = false;
                    } 
                    // if this is the character we picked, then mark it as taken.
                    if (playerConnectionData.ValueRO.CharacterSelected == prepickedCharacter.ValueRO.Character)
                    {
                        prepickedCharacter.ValueRW.OwnerNetworkId = playerConnectionData.ValueRO.OwnerNetworkId;
                        prepickedCharacter.ValueRW.PrePicked = true;
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}



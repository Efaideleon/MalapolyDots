using Assets.Scripts.DOTS.DataComponents;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct CreatePlayerConnectionDataEntity : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (_, netId, entity) in SystemAPI.Query<RefRO<NetworkStreamInGame>, RefRO<NetworkId>>().WithEntityAccess().WithNone<PlayerConnectionDataCreatedTag>())
            {
                var playerDataEntity = ecb.CreateEntity();
                ecb.AddComponent(playerDataEntity, new PlayerConnectionData { CharacterSelected = CharactersEnum.Default, IsLockedIn = false, Owner = netId.ValueRO.Value });
                UnityEngine.Debug.Log($"[CreatePlayerConnectionDataEntity] | Creating Player Connection Entity");
                ecb.AddComponent<PlayerConnectionDataCreatedTag>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct PlayerConnectionData : IComponentData
    {
        public CharactersEnum CharacterSelected;
        public int Owner;
        public bool IsLockedIn;
    }

    public struct PlayerConnectionDataCreatedTag : IComponentData
    { }
}

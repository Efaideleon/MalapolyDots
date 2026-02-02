using Assets.Scripts.DOTS.DataComponents;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.Authorings;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.NetworkSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct InitializeUICharacterDataSystem : ISystem
    {
        EntityQuery initializedQuery;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<PrepickedCharacterReference>();
            initializedQuery = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PrepickedCharactersInitializedTag>());
        }

        public void OnUpdate(ref SystemState state)
        {
            // Run once.
            if (!initializedQuery.IsEmpty)
                return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var references = SystemAPI.GetSingletonBuffer<PrepickedCharacterReference>();

            foreach (var ghosts in references)
            {
                ecb.Instantiate(ghosts.Prefab);
            }

            var initializedEnitity = ecb.CreateEntity();
            ecb.AddComponent<PrepickedCharactersInitializedTag>(initializedEnitity);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct PrepickedCharactersInitializedTag : IComponentData
    { }

    [GhostComponent]
    public struct PrepickedCharacter : IComponentData
    {
        [GhostField]
        public Entity Prefab;

        [GhostField]
        public CharactersEnum Character;

        [GhostField]
        public int OwnerNetworkId;

        [GhostField]
        public bool PrePicked;
    }
}

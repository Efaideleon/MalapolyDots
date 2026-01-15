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
        EntityQuery query;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<PrepickedCharacterReference>();
            initializedQuery = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PrepickedCharactersInitializedTag>());
            query = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<PrepickedCharacter>());
        }

        public void OnUpdate(ref SystemState state)
        {
            // Run once.
            if (!initializedQuery.IsEmpty)
                return;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var ghostPrefab = SystemAPI.GetSingleton<PrepickedCharacterReference>().Prefab;

            int i = 1;
            foreach (CharactersEnum character in System.Enum.GetValues(typeof(CharactersEnum)))
            {
                if (character != CharactersEnum.Default)
                {
                    var entity = ecb.Instantiate(ghostPrefab);
                    UnityEngine.Debug.Log($"[InitializeUICharacterDataSystem] | num of ghost created: {i}");
                    UnityEngine.Debug.Log($"[InitializeUICharacterDataSystem] | character : {character.ToString()}");
                    ecb.SetComponent(entity, new PrepickedCharacter { Character = character, PrePicked = false, Owner = default });
                }
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
        public CharactersEnum Character;

        [GhostField]
        public int Owner;

        [GhostField]
        public bool PrePicked;
    }
}

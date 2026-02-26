using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Systems;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.UI.Authorings;
using DOTS.Characters.CharacterSpawner;
using Unity.Entities;
using Unity.NetCode;

namespace Assets.Scripts.DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct CheckAllGhostCharactersInitializedSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<CharactersSpawnedTag>();
            state.RequireForUpdate<GeneralGhostStates>();
            state.RequireForUpdate<PrepickedCharacter>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // TODO: Eventually use the number from the host server.
            int totalPrepickCharacters = 0;
            foreach (var prepickedCharacter in SystemAPI.Query<RefRO<PrepickedCharacter>>())
            {
                if (prepickedCharacter.ValueRO.PrePicked)
                {
                    totalPrepickCharacters++;
                }
            }

            int numberOfCharSpawned = 0;
            foreach (var _ in SystemAPI.Query<RefRO<CharacterFlag>>())
            {
                numberOfCharSpawned++;
            }

            if (totalPrepickCharacters == numberOfCharSpawned)
            {
                ref var generalGhostStatesEntity = ref SystemAPI.GetSingletonRW<GeneralGhostStates>().ValueRW;
                generalGhostStatesEntity.AllGhostCharactersSpawned = true;
                generalGhostStatesEntity.TotalNumberOfCharSpawned = numberOfCharSpawned;
                UnityEngine.Debug.Log($"[CheckAllGhostCharactersInitializedSystem] | All characters instantiated: picked: {totalPrepickCharacters} spawn: {numberOfCharSpawned}");
                state.Enabled = false;
            }
        }
    }
}

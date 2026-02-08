using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings;
using DOTS.Characters;
using DOTS.Characters.CharacterSpawner;
using DOTS.DataComponents;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameBoardInitializerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Initializing the Current Player ID
            state.RequireForUpdate<CharactersSpawnedTag>();
            state.RequireForUpdate<GamePhaseGhostComponent>();
            state.RequireForUpdate<CurrentPlayerID>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<PlayerID>();
            state.RequireForUpdate<NetworkStreamInGame>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<GameBoardInitializedTag>())
                return;

            if (SystemAPI.GetSingleton<GamePhaseGhostComponent>().GamePhase == GamePhase.Game)
            {
                UnityEngine.Debug.Log($"[GameBoardInitializerSystem] | Creating current active player ghost and id ghost ..");

                foreach (var (ghostOwner, _, e) in SystemAPI.Query<RefRO<GhostOwner>, RefRO<CharacterFlag>>().WithEntityAccess())
                {
                    if (ghostOwner.ValueRO.NetworkId == 1)
                    {
                        var currentPlayerID = ghostOwner.ValueRO.NetworkId;
                        state.EntityManager.CreateSingleton(new CurrentPlayerComponent { entity = e });

                        var currentPlayerIDEntity = SystemAPI.GetSingletonEntity<CurrentPlayerID>();
                        state.EntityManager.SetComponentData(currentPlayerIDEntity, new CurrentPlayerID { Value = currentPlayerID });

                        var currentActivePlayerEntity = SystemAPI.GetSingletonEntity<CurrentActivePlayer>();
                        state.EntityManager.SetComponentData(currentActivePlayerEntity, new CurrentActivePlayer { Entity = e });
                    }
                }

                state.EntityManager.CreateSingleton<GameBoardInitializedTag>();
            }
        }
    }

    public struct GameBoardInitializedTag : IComponentData
    { }
}

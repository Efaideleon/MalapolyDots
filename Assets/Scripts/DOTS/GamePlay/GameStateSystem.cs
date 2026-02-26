using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters.CharacterSpawner;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay
{
    public enum GameState
    {
        Rolling,
        Walking,
        Landing,
    }

    public struct TurnChangedFlag : IComponentData
    {
        public bool Flag;
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GamePlaySystem : ISystem
    {
        public ComponentLookup<FinalArrived> finalArrivedLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RollAmountComponent>();
            state.RequireForUpdate<CharactersSpawnedTag>();
            state.RequireForUpdate<GameStateComponent>();

            finalArrivedLookup = SystemAPI.GetComponentLookup<FinalArrived>();

            state.RequireForUpdate<CurrentActivePlayer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            finalArrivedLookup.Update(ref state);
            var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            if (activePlayerEntity == null)
            {
                return;
            }

            foreach (var playerID in SystemAPI.Query<RefRO<CurrentPlayerID>>().WithChangeFilter<CurrentPlayerID>())
            {
                foreach (var gameState in SystemAPI.Query<RefRW<GameStateComponent>>())
                {
                    gameState.ValueRW.State = GameState.Rolling;
                }
            }

            foreach (var rollComponent in SystemAPI.Query<RefRO<RollAmountComponent>>().WithChangeFilter<RollAmountComponent>())
            {
                UnityEngine.Debug.Log($"[GamePlaySystem] | RollAmountChanged");
                if (rollComponent.ValueRO.Value > 0)
                {
                    foreach (var gameState in SystemAPI.Query<RefRW<GameStateComponent>>())
                    {
                        UnityEngine.Debug.Log($"[GamePlaySystem] | Setting gamestate to walking");
                        gameState.ValueRW.State = GameState.Walking;
                    }
                }
            }

            if (!finalArrivedLookup.HasComponent(activePlayerEntity)) 
                return;

            if (finalArrivedLookup.DidChange(activePlayerEntity, state.LastSystemVersion))
            {
                var arrived = finalArrivedLookup.GetRefRW(activePlayerEntity);
                if (arrived.ValueRO.Value == true)
                {
                        SystemAPI.GetSingletonRW<GameStateComponent>().ValueRW.State = GameState.Landing;
                        arrived.ValueRW.Value = false;
                }
            }
        }
    }
}

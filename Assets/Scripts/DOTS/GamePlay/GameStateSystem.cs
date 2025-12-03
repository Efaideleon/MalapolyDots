using DOTS.Characters;
using DOTS.Characters.CharacterSpawner;
using Unity.Burst;
using Unity.Entities;

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

    public struct GameStateComponent : IComponentData
    {
        public GameState State;
    }

    [BurstCompile]
    public partial struct GamePlaySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RollAmountComponent>();
            state.RequireForUpdate<SpawnFlag>();

            var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
                    {
                    ComponentType.ReadOnly<GameStateComponent>(),
                    });

            SystemAPI.SetComponent(entity, new GameStateComponent
                    {
                    State = GameState.Rolling,
                    });

            state.RequireForUpdate<GameStateComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var playerID in SystemAPI.Query<RefRO<CurrentPlayerID>>().WithChangeFilter<CurrentPlayerID>())
            {
                foreach (var gameState in SystemAPI.Query<RefRW<GameStateComponent>>())
                {
                    gameState.ValueRW.State = GameState.Rolling;
                }
            }

            foreach (var rollComponent in SystemAPI.Query<RefRO<RollAmountComponent>>().WithChangeFilter<RollAmountComponent>())
            {
                if (rollComponent.ValueRO.Value > 0)
                {
                    foreach (var gameState in SystemAPI.Query<RefRW<GameStateComponent>>())
                    {
                        gameState.ValueRW.State = GameState.Walking;
                    }
                }
            }

            foreach (var (arrived, _) in SystemAPI.Query<RefRW<FinalArrived>, RefRO<ActivePlayer>>().WithChangeFilter<FinalArrived>())
            {
                if (arrived.ValueRO.Value == true)
                {
                    foreach (var gameState in SystemAPI.Query<RefRW<GameStateComponent>>())
                    {
                        gameState.ValueRW.State = GameState.Landing;
                        arrived.ValueRW.Value = false;
                    }
                }
            }
        }
    }
}

using Unity.Entities;
using UnityEngine;

public enum GameState
{
    Rolling,
    Walking,
    Transaction,
}

public struct GameStateComponent : IComponentData
{
    public GameState State;
}

public partial struct GamePlaySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TurnComponent>();
        state.RequireForUpdate<RollAmountComponent>();

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

    public void OnUpdate(ref SystemState state)
    {
        foreach (var gameState in SystemAPI.Query<RefRW<GameStateComponent>>())
        {
            foreach (var _ in SystemAPI.Query<RefRO<TurnComponent>>().WithChangeFilter<TurnComponent>())
            {
                gameState.ValueRW.State = GameState.Rolling;
                Debug.Log($"State: {gameState.ValueRW.State}");
            }
            foreach (var _ in SystemAPI.Query<RefRO<RollAmountComponent>>().WithChangeFilter<RollAmountComponent>())
            {
                gameState.ValueRW.State = GameState.Walking;
                Debug.Log($"State: {gameState.ValueRW.State}");
            }
        }
    }
}

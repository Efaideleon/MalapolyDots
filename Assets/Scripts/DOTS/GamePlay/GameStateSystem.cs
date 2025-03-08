using Unity.Entities;
using UnityEngine;

public enum GameState
{
    Rolling,
    Walking,
    Transaction,
}

public struct TurnChangedFlag : IComponentData
{
    public bool Flag;
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

    public void OnUpdate(ref SystemState state)
    {
        foreach (var turnComponent in SystemAPI.Query<RefRO<SpawnFlag>>().WithChangeFilter<SpawnFlag>())
        {
            foreach (var gameState in SystemAPI.Query<RefRW<GameStateComponent>>())
            {
                gameState.ValueRW.State = GameState.Rolling;
                Debug.Log($"State: {gameState.ValueRW.State}");
            }
        }

        foreach (var rollComponent in SystemAPI.Query<RefRO<RollAmountComponent>>().WithChangeFilter<RollAmountComponent>())
        {
            if (rollComponent.ValueRO.Amount > 0)
            {
                foreach (var gameState in SystemAPI.Query<RefRW<GameStateComponent>>())
                {
                    gameState.ValueRW.State = GameState.Walking;
                    Debug.Log($"State: {gameState.ValueRW.State}");
                }
            }
        }
    }
}

using Unity.Entities;

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

    }
}

using Unity.Entities;

public partial struct GamePlaySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TurnComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        /*foreach (var (turnComponent, nameComponet, entity) in SystemAPI.Query<RefRW<TurnComponent>, RefRO<NameDataComponent>>().WithEntityAccess())*/
        /*{*/
        /*}*/
    }
}

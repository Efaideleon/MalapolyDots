using Unity.Entities;

public struct GameBoard : IComponentData
{
    public Entity currentAcitvePlayer;
}

public partial struct GameBoardSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<GameBoard>(),
        });

        /*SystemAPI.SetComponent(entity, new GameBoard*/
        /*{*/
        /*    currentAcitvePlayer = */
        /*});*/
    }

    public void OnUpdate(ref SystemState state)
    {}
}

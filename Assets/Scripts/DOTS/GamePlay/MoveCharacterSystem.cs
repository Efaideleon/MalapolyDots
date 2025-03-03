using Unity.Entities;
using Unity.Transforms;

public partial struct MoveCharacterSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {}

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (turnComponent, entity) in SystemAPI.Query<RefRO<TurnComponent>>().WithEntityAccess())
        {
            if (turnComponent.ValueRO.IsActive)
            {
                foreach (var transform in SystemAPI.Query<RefRW<LocalTransform>>())
                {

                }
            }
        }
    }
}

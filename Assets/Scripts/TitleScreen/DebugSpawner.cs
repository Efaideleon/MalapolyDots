using Unity.Collections;
using Unity.Entities;

public partial struct DebugSpawner : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<DebugStruct>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new(Allocator.Temp);
        foreach ( var (_, entity) in SystemAPI.Query<RefRO<DebugStruct>>().WithEntityAccess())
        {
            var instance = ecb.Instantiate(entity);
        }
        ecb.Playback(state.EntityManager);
        state.Enabled = false;
    }
}

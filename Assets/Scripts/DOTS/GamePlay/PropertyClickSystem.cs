using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct ClickedPropertyComponent : IComponentData
{
    public Entity entity;
}

public partial struct PropertyClickSystem : ISystem
{
    public void OnCreate(ref SystemState state) 
    {
        // Q: Does using CreateSingleton cause structural changes?
        state.EntityManager.CreateSingleton<ClickedPropertyComponent>();
        state.RequireForUpdate<ClickRayCastData>();
        state.RequireForUpdate<ClickedPropertyComponent>();
    }
    public void OnUpdate(ref SystemState state) 
    {
        PhysicsWorld world = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>().ValueRW.PhysicsWorld;
        var collisionWorld = world.CollisionWorld;

        // doesn't handle double tapping the same stop
        foreach (var clickRayCastData in SystemAPI.Query<RefRO<ClickRayCastData>>().WithChangeFilter<ClickRayCastData>())
        {
            RaycastInput input = new()
            {
                Start = clickRayCastData.ValueRO.RayOrigin,
                End = clickRayCastData.ValueRO.RayEnd,
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                }
            };
            if (collisionWorld.CastRay(input, out RaycastHit hit))
            {
                float3 hitPosition = clickRayCastData.ValueRO.RayOrigin + (clickRayCastData.ValueRO.RayDirection * hit.Fraction);
                // Bug: Might click on an enitity that doesn't have a NameComponent
                // TODO: Filter this click for just Properties
                var name = SystemAPI.GetComponent<NameComponent>(hit.Entity);

                UnityEngine.Debug.Log($"Name {name.Value}");
                SystemAPI.SetSingleton(new ClickedPropertyComponent { entity = hit.Entity });
                // TODO: Get this name to a system that handles ui Elements Appearing
                // Use an Event like system?
                // Or reset the component in the receiving system
            }
        }
    }
}

using Unity.Burst;
using Unity.Entities;

[BurstCompile]
[UpdateBefore(typeof(GameUICanvasSystem))]
public partial struct SpaceDetectorSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {   
        state.RequireForUpdate<BoardIndexComponent>(); 
        state.RequireForUpdate<PlayerID>();
        state.RequireForUpdate<PlayerWaypointIndex>();
        state.RequireForUpdate<CurrPlayerID>();

        var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<LandedOnSpace>()
        });

        SystemAPI.SetComponent(entity, new LandedOnSpace { entity = Entity.Null });
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (playerID, playerWaypointIndex) in 
                SystemAPI.Query<RefRO<PlayerID>, RefRO<PlayerWaypointIndex>>()
                .WithChangeFilter<PlayerWaypointIndex>())
        {
            var currentPlayerID = SystemAPI.GetSingleton<CurrPlayerID>();
            foreach (var (boardIndex, spaceEntity) in SystemAPI.Query<RefRO<BoardIndexComponent>>().WithEntityAccess())
            {
                if (playerID.ValueRO.Value == currentPlayerID.Value 
                        && playerWaypointIndex.ValueRO.Value == boardIndex.ValueRO.Value)
                {
                    var spaceLandedEntity = SystemAPI.GetSingletonRW<LandedOnSpace>();
                    spaceLandedEntity.ValueRW.entity = spaceEntity;
                }
            }
        }
    }
}

public struct LandedOnSpace : IComponentData
{
    public Entity entity;
}


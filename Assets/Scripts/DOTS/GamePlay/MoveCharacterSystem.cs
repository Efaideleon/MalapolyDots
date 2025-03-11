using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct ArrivedFlag : IComponentData
{
    public bool Arrived;
}

[BurstCompile]
public partial struct MoveCharacterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TurnComponent>();
        state.RequireForUpdate<RollAmountComponent>();
        state.RequireForUpdate<GameStateComponent>();
        state.RequireForUpdate<WayPointsTag>();

        var entity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
        {
            ComponentType.ReadOnly<ArrivedFlag>()
        });

        SystemAPI.SetComponent(entity, new ArrivedFlag
        {
            Arrived = false
        });
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        var moveSpeed = dt * 5f;

        var currGameState = SystemAPI.GetSingleton<GameStateComponent>();
        if (currGameState.State == GameState.Walking)
        {
            foreach (
                    var (turnComponent, characterWaypoint, localTransform) in
                    SystemAPI.Query<
                    RefRO<TurnComponent>,
                    RefRW<WayPointsBufferIndex>,
                    RefRW<LocalTransform>
                    >())
            {
                if (turnComponent.ValueRO.IsActive)
                {
                    var rollData = SystemAPI.GetSingleton<RollAmountComponent>();
                    var wayPointsBuffer = SystemAPI.GetSingletonBuffer<WayPointBufferElement>();
                    int numOfWayPoints = wayPointsBuffer.Length;
                    int newWayPointIndex = (rollData.Amount + characterWaypoint.ValueRO.Index) % numOfWayPoints;
                    var targetPosition = wayPointsBuffer[newWayPointIndex].WayPoint;

                    if (MoveToTarget(ref localTransform.ValueRW, targetPosition, moveSpeed))
                    {
                        characterWaypoint.ValueRW.Index = newWayPointIndex;
                        foreach (var arrivedFlag in SystemAPI.Query<RefRW<ArrivedFlag>>())
                        {
                            arrivedFlag.ValueRW.Arrived = true;
                        }
                    }
                }
            }
        }
    }

    [BurstCompile]
    private readonly bool MoveToTarget(ref LocalTransform characterTransform, float3 targetPosition, float moveSpeed)
    {
        var pos = characterTransform.Position;
        var dir = targetPosition - pos;

        var moveDirectionNormalized = math.normalizesafe(dir);

        if (math.distance(characterTransform.Position, targetPosition) < 0.1)
        {
            characterTransform.Position = targetPosition;
            return true;
        }

        characterTransform.Position += moveDirectionNormalized * moveSpeed;
        characterTransform.Rotation = quaternion.identity;
        characterTransform.Scale = 1f;
        return false;
    }
}

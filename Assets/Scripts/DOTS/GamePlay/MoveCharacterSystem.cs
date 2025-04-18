using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct ArrivedFlag : IComponentData
{
    public bool Arrived;
}

public struct IsCurrentCharacterMoving : IComponentData
{
    public bool Value;
}

[BurstCompile]
public partial struct MoveCharacterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RollAmountComponent>();
        state.RequireForUpdate<GameStateComponent>();
        state.RequireForUpdate<WayPointsTag>();
        state.RequireForUpdate<CurrentPlayerID>();

        state.EntityManager.CreateSingleton(new IsCurrentCharacterMoving { Value = false });
        state.EntityManager.CreateSingleton(new ArrivedFlag { Arrived = false });
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        var moveSpeed = dt * 5f;

        var currGameState = SystemAPI.GetSingleton<GameStateComponent>();
        if (currGameState.State == GameState.Walking)
        {
            var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>().Value;
            var isCurrentCharacterMoving = SystemAPI.GetSingletonRW<IsCurrentCharacterMoving>();

            foreach (
                    var (playerID, characterWaypoint, localTransform, playerEntity) in
                    SystemAPI.Query<
                    RefRO<PlayerID>,
                    RefRO<PlayerWaypointIndex>,
                    RefRW<LocalTransform>
                    >().WithEntityAccess())
            {
                if (playerID.ValueRO.Value == currentPlayerID)
                {
                    var rollData = SystemAPI.GetSingleton<RollAmountComponent>();
                    var wayPointsBuffer = SystemAPI.GetSingletonBuffer<WayPointBufferElement>();
                    int numOfWayPoints = wayPointsBuffer.Length;
                    int newWayPointIndex = (rollData.AmountRolled + characterWaypoint.ValueRO.Value) % numOfWayPoints;
                    var targetPosition = wayPointsBuffer[newWayPointIndex].WayPoint;
                    isCurrentCharacterMoving.ValueRW.Value = true;

                    if (MoveToTarget(ref localTransform.ValueRW, targetPosition, moveSpeed))
                    {
                        isCurrentCharacterMoving.ValueRW.Value = false;
                        var characterWaypointRW = SystemAPI.GetComponentRW<PlayerWaypointIndex>(playerEntity);
                        characterWaypointRW.ValueRW.Value = newWayPointIndex;
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

using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct MoveCharacterSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TurnComponent>();
        state.RequireForUpdate<RollAmountComponent>();
        state.RequireForUpdate<WayPointsTag>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        var moveSpeed = dt * 5f;

        foreach (
                var (
                    turnComponent,
                    characterWaypoint,
                    localTransform
                ) in
                SystemAPI.Query<
                    RefRO<TurnComponent>,
                    RefRW<WayPointsBufferIndex>,
                    RefRW<LocalTransform>
                    >())
        {
            if (turnComponent.ValueRO.IsActive)
            {
                // Only move when the roll amount has changed.
                var rollData = SystemAPI.GetSingleton<RollAmountComponent>();
                var wayPointsBuffer = SystemAPI.GetSingletonBuffer<WayPointBufferElement>();
                int numOfWayPoints = wayPointsBuffer.Length;
                int newWayPointIndex = (rollData.Amount + characterWaypoint.ValueRO.Index) % numOfWayPoints;
                var targetPosition = wayPointsBuffer[newWayPointIndex].WayPoint;

                UnityEngine.Debug.Log($"taget position: {targetPosition}");
                if (MoveToTarget(ref localTransform.ValueRW, targetPosition, moveSpeed))
                {
                    UnityEngine.Debug.Log($"reached taget position: {targetPosition}");
                    characterWaypoint.ValueRW.Index = newWayPointIndex;
                }
            }
        }
    }
    private bool MoveToTarget(ref LocalTransform characterTransform, float3 targetPosition, float moveSpeed)
    {
        var pos = characterTransform.Position;
        var dir = pos - targetPosition;

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

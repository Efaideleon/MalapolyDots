using DOTS.Characters;
using DOTS.Characters.CharacterSpawner;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct CharacterWaypointSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaypointsBlobRef>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<PlayerWaypointIndex>();
            state.RequireForUpdate<PlayerBoardIndex>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            var moveState = SystemAPI.GetComponentRW<PlayerMovementState>(activePlayerEntity);

            if (moveState.ValueRO.Value != MoveState.Walking) return;

            var waypointsRef = SystemAPI.GetSingleton<WaypointsBlobRef>().Reference;
            var currentWaypointIndex = SystemAPI.GetComponentRW<PlayerWaypointIndex>(activePlayerEntity);
            var playerBoardIndex = SystemAPI.GetComponentRW<PlayerBoardIndex>(activePlayerEntity);
            var targetPosition = SystemAPI.GetComponentRW<TargetPosition>(activePlayerEntity);
            var localTransform = SystemAPI.GetComponentRW<LocalTransform>(activePlayerEntity);
            var finalArrived = SystemAPI.GetComponentRW<FinalArrived>(activePlayerEntity);
            var rollCount = SystemAPI.GetComponentRW<RollCount>(activePlayerEntity);

            ref var waypointsBloblAsset = ref waypointsRef.Value;
            ref var waypoints = ref waypointsBloblAsset.Waypoints;
            int targetWayPointIndex = (currentWaypointIndex.ValueRO.Value + 1) % waypoints.Length;
            var target = waypoints[targetWayPointIndex];
            targetPosition.ValueRW.Value = target.Position;

            if (math.distancesq(localTransform.ValueRO.Position, targetPosition.ValueRO.Value) < 0.001)
            {
                currentWaypointIndex.ValueRW.Value = targetWayPointIndex;
                if (target.IsLandingSpot)
                {
                    rollCount.ValueRW.Value -= 1;
                    playerBoardIndex.ValueRW.Value = (playerBoardIndex.ValueRW.Value + 1) % 40;

                    if (rollCount.ValueRO.Value == 0)
                    {
                        finalArrived.ValueRW.Value = true;
                        moveState.ValueRW.Value = MoveState.Idle;
                    }
                }
            }
        }
    }
}

using DOTS.Characters;
using DOTS.Characters.CharacterSpawner;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct CharacterWaypointSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ReachedTargetPosition>();
            state.RequireForUpdate<ActivePlayer>();
            state.RequireForUpdate<WaypointsBlobRef>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new WaypointLogicJob
            {
                waypointsRef = SystemAPI.GetSingleton<WaypointsBlobRef>().Reference
            }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct WaypointLogicJob : IJobEntity
        {
            [ReadOnly] public BlobAssetReference<WaypointsBlobAsset> waypointsRef;

            public void Execute(
                    ref LocalTransform localTransform,
                    ref PlayerWaypointIndex currentWaypointIndex,
                    ref RollCount rollCount,
                    ref FinalArrived finalArrived,
                    ref PlayerMovementState moveState,
                    ref TargetPosition targetPosition,
                    in ActivePlayer _)
            {
                if (moveState.Value != MoveState.Walking) return;

                ref var waypointsBloblAsset = ref waypointsRef.Value;
                ref var waypoints = ref waypointsBloblAsset.Waypoints;
                int targetWayPointIndex = (currentWaypointIndex.Value + 1) % waypoints.Length;
                var target = waypoints[targetWayPointIndex];
                targetPosition.Value = target.Position;

                if (math.distancesq(localTransform.Position, targetPosition.Value) < 0.001)
                {
                    currentWaypointIndex.Value = targetWayPointIndex;
                    if (target.IsLandingSpot)
                    {
                        rollCount.Value -= 1;

                        if (rollCount.Value == 0)
                        {
                            finalArrived.Value = true;
                            moveState.Value = MoveState.Idle;
                        }
                    }
                }
            }
        }
    }
}

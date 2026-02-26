using DOTS.Characters.CharacterSpawner;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.Characters;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct CharacterWaypointSystem : ISystem
    {
        private const float ReachedWaypointDistanceSq = 0.001f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaypointsBlobRef>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<PlayerWaypointIndex>();
            state.RequireForUpdate<PlayerBoardIndex>();
            state.RequireForUpdate<PlayerArrivedAtDestinationEvent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            if (activePlayerEntity == default)
            {
                return;
            }

            var moveState = SystemAPI.GetComponentRW<PlayerMovementState>(activePlayerEntity);

            if (moveState.ValueRO.Value != MoveState.Walking) return;

            var waypointsRef = SystemAPI.GetSingleton<WaypointsBlobRef>().Reference;
            var currentWaypointIndex = SystemAPI.GetComponentRW<PlayerWaypointIndex>(activePlayerEntity);
            var targetPosition = SystemAPI.GetComponentRW<TargetPosition>(activePlayerEntity);
            var localTransform = SystemAPI.GetComponent<LocalTransform>(activePlayerEntity);

            ref var waypointsBloblAsset = ref waypointsRef.Value;
            ref var waypoints = ref waypointsBloblAsset.Waypoints;
            int currentIndex = currentWaypointIndex.ValueRO.Value;
            int targetWayPointIndex = (currentIndex + 1) % waypoints.Length;
            var target = waypoints[targetWayPointIndex];

            if (math.distancesq(localTransform.Position, target.Position) < ReachedWaypointDistanceSq)
            {
                currentIndex = targetWayPointIndex;
                currentWaypointIndex.ValueRW.Value = currentIndex;
                if (target.IsLandingSpot)
                {
                    var rollCount = SystemAPI.GetComponentRW<RemainingMoves>(activePlayerEntity);
                    rollCount.ValueRW.Value -= 1;

                    var playerBoardIndex = SystemAPI.GetComponentRW<PlayerBoardIndex>(activePlayerEntity);
                    playerBoardIndex.ValueRW.Value = (playerBoardIndex.ValueRW.Value + 1) % 40;

                    if (rollCount.ValueRO.Value == 0)
                    {
                        var finalArrived = SystemAPI.GetComponentRW<FinalArrived>(activePlayerEntity);
                        finalArrived.ValueRW.Value = true;
                        SystemAPI.GetSingletonBuffer<PlayerArrivedAtDestinationEvent>().Add(new PlayerArrivedAtDestinationEvent { });

                        moveState.ValueRW.Value = MoveState.Idle;
                    }
                }

                targetWayPointIndex = (currentIndex + 1) % waypoints.Length;
                target = waypoints[targetWayPointIndex];
            }

            targetPosition.ValueRW.Value = target.Position;
        }
    }
}

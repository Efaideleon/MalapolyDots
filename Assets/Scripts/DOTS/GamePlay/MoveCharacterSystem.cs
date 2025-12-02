using DOTS.Characters;
using DOTS.Characters.CharacterSpawner;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    public struct ArrivedFlag : IComponentData
    {
        public bool Arrived;
    }

    [BurstCompile]
    public partial struct MoveCharacterSystem : ISystem
    {
        private const float moveSpeedFactor = 10f;

        private ComponentLookup<RollAmountCountDown> rollAmountCountDownLookup;
        private ComponentLookup<ArrivedFlag> arrivedFlagLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RollAmountComponent>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<WayPointsTag>();

            rollAmountCountDownLookup = SystemAPI.GetComponentLookup<RollAmountCountDown>();
            arrivedFlagLookup = SystemAPI.GetComponentLookup<ArrivedFlag>();

            state.EntityManager.CreateSingleton(new ArrivedFlag { Arrived = false });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            var moveSpeed = dt * moveSpeedFactor;

            foreach (var rollAmount in SystemAPI.Query<RefRO<RollAmountComponent>>().WithChangeFilter<RollAmountComponent>())
            {
                SystemAPI.GetSingletonRW<RollAmountCountDown>().ValueRW.Value = rollAmount.ValueRO.Value;
            }

            var currGameState = SystemAPI.GetSingleton<GameStateComponent>();
            if (currGameState.State == GameState.Walking)
            {
                rollAmountCountDownLookup.Update(ref state);
                arrivedFlagLookup.Update(ref state);

                new MoveCharacterJob
                {
                    rollAmountCountDownLookup = rollAmountCountDownLookup,
                    arrivedFlagLookup = arrivedFlagLookup,
                    waypoints = SystemAPI.GetSingletonBuffer<WayPointBufferElement>(),
                    rollAmountCountDownEntity = SystemAPI.GetSingletonEntity<RollAmountCountDown>(),
                    arrivedFlagEntity = SystemAPI.GetSingletonEntity<ArrivedFlag>(),
                    moveSpeed = moveSpeed,
                    ecb = GetECB(ref state).AsParallelWriter()
                }.ScheduleParallel();
            }
        }

        public partial struct MoveCharacterJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<RollAmountCountDown> rollAmountCountDownLookup;
            [ReadOnly] public ComponentLookup<ArrivedFlag> arrivedFlagLookup;
            [ReadOnly] public DynamicBuffer<WayPointBufferElement> waypoints;
            [ReadOnly] public Entity rollAmountCountDownEntity;
            [ReadOnly] public Entity arrivedFlagEntity;
            [ReadOnly] public float moveSpeed;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute(
                    [ChunkIndexInQuery] int chunkIndex,
                    ref PlayerWaypointIndex waypointIndex,
                    ref LocalTransform localTransform,
                    ref PlayerMovementState moveState,
                    in ActivePlayer _)
            {
                int targetWayPointIndex = (waypointIndex.Value + 1) % waypoints.Length;
                var target = waypoints[targetWayPointIndex];

                if (moveState.Value != MoveState.Walking)
                {
                    moveState.Value = MoveState.Walking;
                }

                if (MoveToTarget(ref localTransform, ref target.WayPoint, moveSpeed))
                {
                    waypointIndex.Value = targetWayPointIndex;

                    if (target.Name != "None")
                    {
                        if (rollAmountCountDownLookup.HasComponent(rollAmountCountDownEntity))
                        {
                            var rollCount = rollAmountCountDownLookup.GetRefRO(rollAmountCountDownEntity).ValueRO.Value;
                            var newRollCount = rollCount - 1;

                            ecb.SetComponent(chunkIndex, rollAmountCountDownEntity, new RollAmountCountDown { Value = newRollCount });

                            if (newRollCount == 0)
                            {
                                if (arrivedFlagLookup.HasComponent(arrivedFlagEntity))
                                {
                                    ecb.SetComponent(chunkIndex, arrivedFlagEntity, new ArrivedFlag { Arrived = true });
                                    moveState.Value = MoveState.Idle;
                                }
                            }
                        }
                    }

                }
            }
        }

        [BurstCompile]
        private static bool MoveToTarget(ref LocalTransform characterTransform, ref float3 nextTargetPosition, float moveSpeed)
        {
            float3 pos = characterTransform.Position;
            float3 delta = nextTargetPosition - pos;
            float dist = math.length(delta);

            if (dist <= moveSpeed)
            {
                characterTransform.Position = nextTargetPosition;
                return true;
            }
            float3 dir = delta / dist;
            characterTransform.Position += dir * moveSpeed;

            if (math.lengthsq(dir) > float.Epsilon)
            {
                characterTransform.Rotation = quaternion.LookRotationSafe(dir, math.up());
            }
            return false;
        }

        [BurstCompile]
        public readonly EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecb.CreateCommandBuffer(state.WorldUnmanaged);
        }
    }
}

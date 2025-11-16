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

    public struct IsCurrentCharacterMoving : IComponentData
    {
        public bool Value;
    }

    public struct FinalWayPointIndex : IComponentData
    {
        public int Value;
    }

    [BurstCompile]
    public partial struct MoveCharacterSystem : ISystem
    {
        private const float moveSpeedFactor = 10f;
        //private const float moveSpeedFactor = 50f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RollAmountComponent>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<WayPointsTag>();
            state.RequireForUpdate<CurrentPlayerID>();
            state.RequireForUpdate<FinalWayPointIndex>();

            state.EntityManager.CreateSingleton(new IsCurrentCharacterMoving { Value = false });
            state.EntityManager.CreateSingleton(new ArrivedFlag { Arrived = false });
            state.EntityManager.CreateSingleton(new FinalWayPointIndex { Value = default });
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
                var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>().Value;
                var isCurrentCharacterMoving = SystemAPI.GetSingletonRW<IsCurrentCharacterMoving>();

                foreach (
                        var (playerID, characterWaypoint, localTransform, playerEntity) in
                        SystemAPI.Query<
                        RefRO<PlayerID>,
                        RefRO<PlayerWaypointIndex>,
                        RefRW<LocalTransform>
                        >()
                        .WithEntityAccess())
                {
                    if (playerID.ValueRO.Value == currentPlayerID)
                    {
                        var wayPoints = SystemAPI.GetSingletonBuffer<WayPointBufferElement>();
                        int targetWayPointIndex = (characterWaypoint.ValueRO.Value + 1) % wayPoints.Length;
                        var target = wayPoints[targetWayPointIndex];
                        ref var rollAmountCountRef = ref SystemAPI.GetSingletonRW<RollAmountCountDown>().ValueRW;
                        var playerMovementState = SystemAPI.GetComponent<PlayerMovementState>(playerEntity);

                        isCurrentCharacterMoving.ValueRW.Value = true;
                        if (playerMovementState.Value != MoveState.Walking)
                        {
                            SystemAPI.GetComponentRW<PlayerMovementState>(playerEntity).ValueRW.Value = MoveState.Walking;
                        }

                        if (MoveToTarget(ref localTransform.ValueRW, target.WayPoint, moveSpeed))
                        {
                            SystemAPI.GetComponentRW<PlayerWaypointIndex>(playerEntity).ValueRW.Value = targetWayPointIndex;

                            UpdateRollCountDown(ref target.Name, ref rollAmountCountRef);

                            if (rollAmountCountRef.Value == 0)
                            {
                                isCurrentCharacterMoving.ValueRW.Value = false;
                                SystemAPI.GetSingletonRW<ArrivedFlag>().ValueRW.Arrived = true;
                                SystemAPI.GetComponentRW<PlayerMovementState>(playerEntity).ValueRW.Value = MoveState.Idle;
                            }
                        }
                    }
                }
            }
        }

        [BurstCompile]
        private readonly void UpdateRollCountDown(ref FixedString64Bytes targetName, ref RollAmountCountDown rollCount)
        {
            if (targetName != "None")
            {
                rollCount.Value -= 1;
            }
        }


        [BurstCompile]
        private readonly bool MoveToTarget(ref LocalTransform characterTransform, float3 nextTargetPosition, float moveSpeed)
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
            return false;
        }
    }
}

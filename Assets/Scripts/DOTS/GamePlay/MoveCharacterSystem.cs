using DOTS.Characters;
using DOTS.Characters.CharacterSpawner;
using Unity.Burst;
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
        //private const float moveSpeedFactor = 10f;
        private const float moveSpeedFactor = 50f;

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

            foreach (var rollAmount in 
                    SystemAPI.Query<
                    RefRO<RollAmountComponent>
                    >()
                    .WithChangeFilter<RollAmountComponent>())
            {
                var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();
                foreach (var (playerID, characterWaypoint) in SystemAPI.Query<RefRO<PlayerID>, RefRO<PlayerWaypointIndex>>())
                {
                    if (playerID.ValueRO.Value == currentPlayerID.Value)
                    {
                        var wayPointsBuffer = SystemAPI.GetSingletonBuffer<WayPointBufferElement>();
                        int numOfWayPoints = wayPointsBuffer.Length;
                        int finalWayPointIndex = (rollAmount.ValueRO.Value + characterWaypoint.ValueRO.Value) % numOfWayPoints;
                        SystemAPI.GetSingletonRW<FinalWayPointIndex>().ValueRW.Value = finalWayPointIndex;
                        SystemAPI.GetSingletonRW<RollAmountCountDown>().ValueRW.Value = rollAmount.ValueRO.Value;
                    }
                }
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
                        var wayPointsBuffer = SystemAPI.GetSingletonBuffer<WayPointBufferElement>();
                        int numOfWayPoints = wayPointsBuffer.Length;
                        int nextWayPointIndex = (characterWaypoint.ValueRO.Value + 1) % numOfWayPoints;
                        var nextTargetPosition = wayPointsBuffer[nextWayPointIndex].WayPoint;
                        isCurrentCharacterMoving.ValueRW.Value = true;

                        // We are writing to this component every frame.
                        SystemAPI.GetComponentRW<PlayerMovementState>(playerEntity).ValueRW.Value = MoveState.Walking;
                        if (MoveToTarget(ref localTransform.ValueRW, nextTargetPosition, moveSpeed))
                        {
                            SystemAPI.GetComponentRW<PlayerWaypointIndex>(playerEntity).ValueRW.Value = nextWayPointIndex;
                            int finalWayPointIndex = SystemAPI.GetSingleton<FinalWayPointIndex>().Value;
                            if (nextWayPointIndex == finalWayPointIndex)
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

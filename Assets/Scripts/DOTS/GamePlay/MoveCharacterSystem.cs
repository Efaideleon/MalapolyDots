using DOTS.Characters;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct MoveCharacterSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<CurrentActivePlayer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currGameState = SystemAPI.GetSingleton<GameStateComponent>();
            if (currGameState.State == GameState.Walking)
            {
                var activePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
                var moveStateRW = SystemAPI.GetComponentRW<PlayerMovementState>(activePlayer);
                var localTransformRW = SystemAPI.GetComponentRW<LocalTransform>(activePlayer);
                var targetPosition = SystemAPI.GetComponent<TargetPosition>(activePlayer);
                var moveSpeed = SystemAPI.GetComponent<MoveSpeed>(activePlayer);

                if (moveStateRW.ValueRO.Value != MoveState.Walking)
                {
                    moveStateRW.ValueRW.Value = MoveState.Walking;
                }

                MoveToTarget(ref localTransformRW.ValueRW, in targetPosition.Value, moveSpeed.Value * SystemAPI.Time.DeltaTime);
            }
        }

        [BurstCompile]
        private static bool MoveToTarget(ref LocalTransform characterTransform, in float3 targetPos, float moveSpeed)
        {
            float3 currentPos = characterTransform.Position;
            float3 delta = targetPos - currentPos;
            float distSq = math.lengthsq(delta);

            // When the player is close enough to the target.
            if (distSq <= moveSpeed * moveSpeed)
            {
                characterTransform.Position = targetPos;
                return true;
            }

            var dist = math.sqrt(distSq);
            float3 dir = delta / dist;
            characterTransform.Position += dir * moveSpeed;

            if (distSq > float.Epsilon)
            {
                characterTransform.Rotation = quaternion.LookRotationSafe(dir, math.up());
            }
            return false;
        }
    }
}

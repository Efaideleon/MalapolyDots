using DOTS.Characters;
using Unity.Burst;
using Unity.Collections;
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
            state.RequireForUpdate<RollAmountComponent>();
            state.RequireForUpdate<GameStateComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currGameState = SystemAPI.GetSingleton<GameStateComponent>();
            if (currGameState.State == GameState.Walking)
            {
                new MoveCharacterJob
                {
                    dt = SystemAPI.Time.DeltaTime
                }.ScheduleParallel();
            }
        }

        [BurstCompile]
        public partial struct MoveCharacterJob : IJobEntity
        {
            [ReadOnly] public float dt;

            public void Execute(
                    ref LocalTransform localTransform,
                    ref PlayerMovementState moveState,
                    ref TargetPosition targetPosition,
                    in MoveSpeed moveSpeed,
                    in ActivePlayer _)
            {
                if (moveState.Value != MoveState.Walking)
                {
                    moveState.Value = MoveState.Walking;
                }

                MoveToTarget(ref localTransform, targetPosition.Value, moveSpeed.Value * dt);
            }
        }

        [BurstCompile]
        private static bool MoveToTarget(ref LocalTransform characterTransform, float3 targetPos, float moveSpeed)
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

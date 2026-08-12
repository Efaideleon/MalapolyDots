using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(CharacterWaypointSystem))]
    [BurstCompile]
    public partial struct MoveCharacterSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerMovementState>();
            state.RequireForUpdate<CurrentActivePlayer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var activePlayer = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            if (activePlayer == default)
            {
                return;
            }
            var moveState = SystemAPI.GetComponent<PlayerMovementState>(activePlayer);

            if (moveState.Value != MoveState.Walking)
                return;

            var localTransformRW = SystemAPI.GetComponentRW<LocalTransform>(activePlayer);
            var targetPosition = SystemAPI.GetComponent<TargetPosition>(activePlayer);
            var moveSpeed = SystemAPI.GetComponent<MoveSpeed>(activePlayer);


            MoveToTarget(ref localTransformRW.ValueRW, in targetPosition.Value, moveSpeed.Value * SystemAPI.Time.DeltaTime);
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

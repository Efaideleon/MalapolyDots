using DOTS.DataComponents;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using DOTS.Characters;

namespace DOTS.GamePlay.CameraSystems
{
    [BurstCompile]
    public partial struct CameraSystem : ISystem
    {
        float3 up;
        float3 offset;
        float3 initialOffset;
        float currentAngleDeg;
        int currSpaceIdx;
        bool isAnimating;
        float3 newRotatedOffsetVector;
        int prevPlayerID;
        int currPlayerID;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            up = new float3(0, 1, 0);
            offset = new float3(0f, 13f, 27f);
            currentAngleDeg = 0;
            initialOffset = GetRotatedCameraOffsetVector(offset, math.radians(51));
            currSpaceIdx = 0;
            isAnimating = false;
            prevPlayerID = -1;
            currPlayerID = -1;
            state.RequireForUpdate<PlayerID>();
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<RollAmountCountDown>();
            state.RequireForUpdate<CurrentRound>();
            state.RequireForUpdate<GameStateComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var mainCameraTransform = SystemAPI.GetSingletonRW<MainCameraTransform>();
            var player = SystemAPI.GetSingleton<CurrentPlayerComponent>();

            if (SystemAPI.HasComponent<LocalTransform>(player.entity))
            {
                currPlayerID = SystemAPI.GetComponent<PlayerID>(player.entity).Value;
                var playerLocalTransform = SystemAPI.GetComponent<LocalTransform>(player.entity);
                currSpaceIdx = SystemAPI.GetComponent<PlayerWaypointIndex>(player.entity).Value;
                var playerMoveState = SystemAPI.GetComponent<PlayerMovementState>(player.entity).Value;
                var roundNumber = SystemAPI.GetSingleton<CurrentRound>().Value;

                if (ShouldRotateCamera(currSpaceIdx) && 
                    playerMoveState == MoveState.Walking && 
                    !isAnimating)
                {
                    if (currSpaceIdx == 0 && roundNumber == 0)
                    { }
                    else
                    {
                        isAnimating = true;
                        currentAngleDeg = 0;
                    }
                }
                if (currPlayerID != prevPlayerID)
                {
                    isAnimating = false;
                }

                if (isAnimating)
                {
                    currentAngleDeg = math.lerp(currentAngleDeg, 90, 5f * deltaTime);
                    float interpolatedAngleRadians = math.radians(currentAngleDeg);
                    var nextSpaceIdx = (currSpaceIdx + 1) % 39;
                    newRotatedOffsetVector = GetRotatedCameraOffsetVector(
                            GetRotatedVector(
                                initialOffset, 
                                nextSpaceIdx, 
                                final: false,
                                roundNumber
                            ), 
                            interpolatedAngleRadians);
                }
                else
                {
                    newRotatedOffsetVector = GetRotatedVector(initialOffset, currSpaceIdx, final: true, roundNumber);
                }

                mainCameraTransform.ValueRW.Position = playerLocalTransform.Position + newRotatedOffsetVector;
                float3 forward = math.normalize(playerLocalTransform.Position - mainCameraTransform.ValueRO.Position);
                var quaternionToLookAtPlayer = quaternion.LookRotationSafe(forward, up);
                mainCameraTransform.ValueRW.Rotation = quaternionToLookAtPlayer;
                prevPlayerID = currPlayerID;
            }
        }

        public readonly float3 GetRotatedVector(float3 initialOffset, int spaceIdx, bool final, int round)
        {
            int leftBound;
            if (round == 0)
                leftBound = 0;
            else 
                leftBound = 1;

            if (spaceIdx >= leftBound && spaceIdx <= 10)
                return GetRotatedCameraOffsetVector(initialOffset, math.radians(final ? 90 : 0));
            if (spaceIdx >= 11 && spaceIdx <= 20)
                return GetRotatedCameraOffsetVector(initialOffset, math.radians(final ? 180 : 90));
            if (spaceIdx >= 21 && spaceIdx <= 30)
                return GetRotatedCameraOffsetVector(initialOffset, math.radians(final ? 270 : 180));
            if (spaceIdx >= 31 && spaceIdx <= 39 || spaceIdx == 0)
                return GetRotatedCameraOffsetVector(initialOffset, math.radians(final ? 360 : 270));
            return default;
        }

        // Returns:
        // The angle of the camera in the radians based on the board side
        // Top: side where Mercado is at : 0 - 9
        // Right: side where Santa Lucias is at : 10 - 19
        // Bottom: side where Ineb is at : 20 - 29
        // Left: side where El estadio is at : 30 - 39
        [BurstCompile]
        public readonly bool ShouldRotateCamera(int currSpaceIdx)
        {
            if (currSpaceIdx == 10 || currSpaceIdx == 20 || currSpaceIdx == 30 || currSpaceIdx == 0) 
            {
                return true;
            }
            return false;
        }

        public readonly float3 GetRotatedCameraOffsetVector(float3 offset, float angle)
        {
            var quaternionToRotateVector = quaternion.RotateY(angle);
            return math.mul(quaternionToRotateVector, offset);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        { }
    }
}

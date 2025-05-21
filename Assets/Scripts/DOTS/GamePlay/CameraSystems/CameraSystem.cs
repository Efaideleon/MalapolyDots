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
        int prevSpaceIdx;
        int currSpaceIdx;
        bool isAnimating;
        float3 newRotatedOffsetVector;
        int prevPlayerID;
        int currPlayerID;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            up = new float3(0, 1, 0);
            //                  x    z    y
            offset = new float3(0f, 13f, 27f);
            currentAngleDeg = 0;
            initialOffset = GetRotatedCameraOffsetVector(offset, math.radians(51));
            prevSpaceIdx = 0;
            prevSpaceIdx = 0;
            isAnimating = false;
            prevPlayerID = -1;
            currPlayerID = -1;
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<CurrentPlayerComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var mainCameraTransform = SystemAPI.GetSingletonRW<MainCameraTransform>();
            var player = SystemAPI.GetSingleton<CurrentPlayerComponent>();

            if (SystemAPI.HasComponent<LocalTransform>(player.entity))
            {
                currSpaceIdx = SystemAPI.GetComponent<PlayerWaypointIndex>(player.entity).Value;
                currPlayerID = SystemAPI.GetComponent<PlayerID>(player.entity).Value;
                if (currPlayerID != prevPlayerID)
                {
                    isAnimating = false;
                }
                var playerLocalTransform = SystemAPI.GetComponent<LocalTransform>(player.entity);
                if (ChangeInSides(prevSpaceIdx, currSpaceIdx))
                {
                    isAnimating = true;
                    currentAngleDeg = 0;
                }

                if (isAnimating)
                {
                    currentAngleDeg = math.lerp(currentAngleDeg, 90, 10 * deltaTime);
                    float interpolatedAngleRadians = math.radians(currentAngleDeg);
                    newRotatedOffsetVector = GetRotatedCameraOffsetVector(initialOffset, interpolatedAngleRadians);
                }
                else
                {
                    newRotatedOffsetVector = GetRotatedVector(initialOffset, currSpaceIdx);
                }

                mainCameraTransform.ValueRW.Position = playerLocalTransform.Position + newRotatedOffsetVector;
                float3 forward = math.normalize(playerLocalTransform.Position - mainCameraTransform.ValueRO.Position);
                var quaternionToLookAtPlayer = quaternion.LookRotationSafe(forward, up);
                mainCameraTransform.ValueRW.Rotation = quaternionToLookAtPlayer;
                prevSpaceIdx = currSpaceIdx;
                prevPlayerID =currPlayerID;
                //state.Enabled = false;
            }
        }

        public readonly float3 GetRotatedVector(float3 initialOffset, int spaceIdx)
        {
            return spaceIdx switch 
            {
                >= 0 and <= 10 => GetRotatedCameraOffsetVector(initialOffset, math.radians(90)),
                >= 11 and <= 20 => GetRotatedCameraOffsetVector(initialOffset, math.radians(180)),
                >= 21 and <= 30 => GetRotatedCameraOffsetVector(initialOffset, math.radians(270)),
                >= 31 and <= 39 => GetRotatedCameraOffsetVector(initialOffset, math.radians(360)),
                _ => 0
            };
        }

        // Returns:
        // The angle of the camera in the radians based on the board side
        // Top: side where Mercado is at : 0 - 10
        // Right: side where Santa Lucias is at : 11 - 20
        // Bottom: side where Ineb is at : 21 - 30
        // Left: side where El estadio is at : 31 - 39
        [BurstCompile]
        public readonly bool ChangeInSides(int prevSpaceIdx, int currSpaceIdx)
        {
            if (prevSpaceIdx >= 0 && prevSpaceIdx <= 10 && currSpaceIdx >= 11 && currSpaceIdx <= 20)
                return true;
            if (prevSpaceIdx >= 11 && prevSpaceIdx <= 20 && currSpaceIdx >= 21 && currSpaceIdx <= 30)
                return true;
            if (prevSpaceIdx >= 21 && prevSpaceIdx <= 30 && currSpaceIdx >= 31 && currSpaceIdx <= 39)
                return true;
            if (prevSpaceIdx >= 31 && prevSpaceIdx <= 39 && currSpaceIdx >= 0 && currSpaceIdx <= 10)
                return true;
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

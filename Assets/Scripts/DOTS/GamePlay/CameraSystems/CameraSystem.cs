// ========================================================================
// **Camera Rotation System**
// * Rotates the camera when the player turns on a corner.
// ========================================================================
using DOTS.DataComponents;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using DOTS.Characters;

namespace DOTS.GamePlay.CameraSystems
{
    public struct CameraState : IComponentData
    {
        public float CurrentAngleRad;
        public bool IsAnimating;
        public int PreviousPlayerId;
        public float3 InitialOffset;
        public float RotationThresholdRad;
        public float RotationSpeed;
        public int LastAnimatedSpaceIdx;
        public float3 TargetOffset;
    }

    [BurstCompile]
    public partial struct CameraSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            var offset = new float3(0f, 13f, 27f);
            state.EntityManager.CreateSingleton(new CameraState
            {
                CurrentAngleRad = 0,
                IsAnimating = false,
                PreviousPlayerId = -1,
                InitialOffset = RotateOffset(offset, math.radians(51)),
                RotationThresholdRad = math.PI / 2,
                RotationSpeed = math.PI,
                LastAnimatedSpaceIdx = -1,
                TargetOffset = default
            });
            state.RequireForUpdate<PlayerID>();
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<RollAmountCountDown>();
            state.RequireForUpdate<CurrentRound>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<FreeCameraToggleFlag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var cameraState = SystemAPI.GetSingletonRW<CameraState>();
            ref var camStateRW = ref cameraState.ValueRW;
            var camStateRO = cameraState.ValueRO;
            var deltaTime = SystemAPI.Time.DeltaTime;

            var camTransform = SystemAPI.GetSingletonRW<MainCameraTransform>();
            var currentPlayer = SystemAPI.GetSingleton<CurrentPlayerComponent>();
            if (!SystemAPI.HasComponent<LocalTransform>(currentPlayer.entity))
                return;

            var playerId = SystemAPI.GetComponent<PlayerID>(currentPlayer.entity).Value;
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(currentPlayer.entity);
            var currentSpaceIdx = SystemAPI.GetComponent<PlayerWaypointIndex>(currentPlayer.entity).Value;
            var playerMoveState = SystemAPI.GetComponent<PlayerMovementState>(currentPlayer.entity).Value;
            var roundNum = SystemAPI.GetSingleton<CurrentRound>().Value;

            bool isNewPlayer = playerId != camStateRO.PreviousPlayerId;
            bool atCorner = IsCornerSpace(currentSpaceIdx);
            bool isWalking = playerMoveState == MoveState.Walking;

            bool isAnimating = camStateRO.IsAnimating;
            bool spaceChanged = camStateRO.LastAnimatedSpaceIdx != currentSpaceIdx;

            if (isNewPlayer)
            {
                camStateRW.IsAnimating = false;
                camStateRW.CurrentAngleRad = 0;
                camStateRW.LastAnimatedSpaceIdx = -1;
            }
            else if (ShouldStartAnimation(atCorner, isWalking, currentSpaceIdx, roundNum, camStateRO))
            {
                StartAnimation(ref camStateRW, currentSpaceIdx);
            }

            if (isAnimating)
            {
                camStateRW.TargetOffset = PlayRotationAnimation(currentSpaceIdx, roundNum, deltaTime, ref camStateRW);
            }
            else if (!isAnimating && spaceChanged)
            {
                camStateRW.TargetOffset = ComputeFinalOffset(currentSpaceIdx, roundNum, camStateRO);
            }

            // If free camera is enabled don't set the camera to the player position
            bool isCameraFree = SystemAPI.GetSingleton<FreeCameraToggleFlag>().Value; 
            if (!isCameraFree)
            {
                ApplyCameraTransform(ref camTransform.ValueRW, playerTransform.Position, camStateRO.TargetOffset);
            }
            camStateRW.PreviousPlayerId = playerId;
        }

        [BurstCompile]
        private readonly void ApplyCameraTransform(ref MainCameraTransform cam, float3 playerPos, float3 offset)
        {
            cam.Position = playerPos + offset;
            float3 forward = math.normalize(playerPos - cam.Position);
            cam.Rotation = quaternion.LookRotationSafe(forward, math.up());
        }

        [BurstCompile]
        private readonly float3 PlayRotationAnimation(
                int spaceIdx,
                int roundNum,
                float deltaTime,
                ref CameraState state
        )
        {
            var nextAngle = state.CurrentAngleRad + state.RotationSpeed * deltaTime;

            if (nextAngle >= state.RotationThresholdRad)
            {
                nextAngle = state.RotationThresholdRad;
                state.IsAnimating = false;
            }

            state.CurrentAngleRad = nextAngle;

            var nextSpaceIdx = (spaceIdx + 1) % 39;
            var cornerAngle = FindRotationAngle(nextSpaceIdx, final: false, roundNum);
            var baseOff = RotateOffset(state.InitialOffset, cornerAngle);

            return RotateOffset(baseOff, state.CurrentAngleRad);
        }

        [BurstCompile]
        private readonly float FindRotationAngle(int spaceIdx, bool final, int round)
        {
            int start = round == 0 ? 0 : 1;
            if (spaceIdx >= start && spaceIdx <= 10) return final ? math.PIHALF : 0;
            if (spaceIdx >= 11 && spaceIdx <= 20) return final ? math.PI : math.PIHALF;
            if (spaceIdx >= 21 && spaceIdx <= 30) return final ? 3 * math.PIHALF : math.PI;
            if (spaceIdx >= 31 && spaceIdx <= 39 || spaceIdx == 0) return final ? math.PI2 : 3 * math.PIHALF;

            return default;
        }

        [BurstCompile]
        public readonly float3 ComputeFinalOffset(int spaceIdx, int round, in CameraState state)
        {
            float angle = FindRotationAngle(spaceIdx, final: true, round);
            return RotateOffset(state.InitialOffset, angle);
        }

        [BurstCompile]
        private readonly bool IsCornerSpace(int spaceIdx) => spaceIdx % 10 == 0;

        [BurstCompile]
        private readonly float3 RotateOffset(float3 offset, float angle) => math.mul(quaternion.RotateY(angle), offset);

        [BurstCompile]
        private readonly bool ShouldStartAnimation(bool atCorner, bool isWalking, int spaceIdx, int roundNum, in CameraState state)
        {
            return atCorner
                && isWalking
                && !state.IsAnimating
                && !(spaceIdx == 0 && roundNum == 0)
                && state.LastAnimatedSpaceIdx != spaceIdx;
        }

        [BurstCompile]
        private readonly void StartAnimation(ref CameraState state, int spaceIdx)
        {
            state.IsAnimating = true;
            state.CurrentAngleRad = 0;
            state.LastAnimatedSpaceIdx = spaceIdx;
        }
    }
}

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
       private static readonly float3 Up = new(0, 1, 0);
       private const float RotationThresholdDegree = 90;
       private float3 _offset;
       private float3 _initialOffset;
       private float _currentAngleDeg;
       private bool _isAnimating;
       private float3 _newRotatedOffsetVector;
       private int _prevPlayerID;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _offset = new float3(0f, 13f, 27f);
            _currentAngleDeg = 0;
            _initialOffset = GetRotatedCameraOffsetVector(_offset, math.radians(51));
            _isAnimating = false;
            _prevPlayerID = -1;
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
            var camTransform = SystemAPI.GetSingletonRW<MainCameraTransform>();
            var currentPlayer = SystemAPI.GetSingleton<CurrentPlayerComponent>();
            
            if (!SystemAPI.HasComponent<LocalTransform>(currentPlayer.entity))
                return;

            var playerId = SystemAPI.GetComponent<PlayerID>(currentPlayer.entity).Value;
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(currentPlayer.entity);
            var currentSpaceIdx = SystemAPI.GetComponent<PlayerWaypointIndex>(currentPlayer.entity).Value;
            var playerMoveState = SystemAPI.GetComponent<PlayerMovementState>(currentPlayer.entity).Value;
            var roundNum = SystemAPI.GetSingleton<CurrentRound>().Value;

            bool shouldRotate = ShouldRotateCamera(currentSpaceIdx);
            bool isNewPlayer = playerId != _prevPlayerID;

            if (shouldRotate && 
                playerMoveState == MoveState.Walking && 
                !_isAnimating)
            {
                if (currentSpaceIdx == 0 && roundNum == 0)
                { }
                else
                {
                    _isAnimating = true;
                    _currentAngleDeg = 0;
                }
            }
            if (isNewPlayer)
            {
                _isAnimating = false;
            }

            if (_isAnimating)
            {
                _currentAngleDeg = math.lerp(_currentAngleDeg, RotationThresholdDegree, 5f * deltaTime);
                float interpolatedAngleRadians = math.radians(_currentAngleDeg);
                var nextSpaceIdx = (currentSpaceIdx + 1) % 39;
                _newRotatedOffsetVector = GetRotatedCameraOffsetVector(
                        GetRotatedVector(
                            _initialOffset, 
                            nextSpaceIdx, 
                            final: false,
                            roundNum
                        ), 
                        interpolatedAngleRadians);
            }
            else
            {
                _newRotatedOffsetVector = GetRotatedVector(_initialOffset, currentSpaceIdx, final: true, roundNum);
            }

            camTransform.ValueRW.Position = playerTransform.Position + _newRotatedOffsetVector;
            float3 forward = math.normalize(playerTransform.Position - camTransform.ValueRO.Position);
            var quaternionToLookAtPlayer = quaternion.LookRotationSafe(forward, Up);
            camTransform.ValueRW.Rotation = quaternionToLookAtPlayer;
            _prevPlayerID = playerId;
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

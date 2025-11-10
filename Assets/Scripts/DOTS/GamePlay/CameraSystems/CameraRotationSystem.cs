using DOTS.DataComponents;
using DOTS.GamePlay.CameraSystems.TriggerSimulationEvent;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

#nullable enable
namespace DOTS.GamePlay.CameraSystems
{
    public enum AngleAnimationState
    {
        Finished,
        InProgress
    }

    [UpdateAfter(typeof(CameraSceneManagerSystem))]
    public partial struct CameraRotationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton<AngleAnimationData>();
            state.RequireForUpdate<StatefulTriggerEvent>();
            state.RequireForUpdate<CameraSceneData>();
            state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<MainCameraTransform>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var angleAnimationData = SystemAPI.GetSingletonRW<AngleAnimationData>();
            var playerToCameraAngle = SystemAPI.GetSingletonRW<PlayerToCameraAngleData>();
            var pivotTransform = SystemAPI.GetSingletonRW<PivotTransform>();
            var currentPlayerID = SystemAPI.GetSingleton<CurrentPlayerID>();

            foreach (var buffer in SystemAPI.Query<DynamicBuffer<StatefulTriggerEvent>>().WithChangeFilter<StatefulTriggerEvent>())
            {
                foreach (var statefulEvent in buffer)
                {
                    if (IsCameraStatefulEvent(ref state, statefulEvent, out Entity cameraSceneEntity))
                    {
                        switch (statefulEvent.State)
                        {
                            case StateEventType.Enter: 
                                var cameraZoneData = SystemAPI.GetComponent<CameraSceneData>(cameraSceneEntity);
                                var currentRotation = playerToCameraAngle.ValueRW.Map[currentPlayerID.Value];

                                angleAnimationData.ValueRW.TargetAngleY = math.radians(cameraZoneData.RotationAngleY);
                                angleAnimationData.ValueRW.AnimationState = AngleAnimationState.InProgress;
                                angleAnimationData.ValueRW.RotationSpeed = cameraZoneData.RotationSpeed;
                                angleAnimationData.ValueRW.CurrentAngleY = 0;
                                angleAnimationData.ValueRW.InitialRotation = currentRotation;
                                angleAnimationData.ValueRW.Delta = 0;

                                UnityEngine.Debug.Log($"[CameraRotationSystem] | currentAngle : {currentRotation}");
                                break;
                        }
                    }
                }
            }

            AnimateAngleRotation(
                    ref state,
                    ref angleAnimationData.ValueRW,
                    ref pivotTransform.ValueRW,
                    ref playerToCameraAngle.ValueRW,
                    currentPlayerID.Value,
                    ref deltaTime
            );
        }

        /// <summary>
        /// Animates the angle from the current state
        /// </summary>
        private readonly void AnimateAngleRotation(
                ref SystemState _,
                ref AngleAnimationData angleData,
                ref PivotTransform pivotTransform,
                ref PlayerToCameraAngleData playerToCameraAngle,
                in int playerID,
                ref float dt)
        {
            switch (angleData.AnimationState)
            {
                case AngleAnimationState.InProgress:

                    angleData.Delta += angleData.RotationSpeed * dt;
                    float t = math.clamp(angleData.Delta, 0, 1);

                    if (t < 1) // TODO: how would it work for negative angles.
                    {
                        var targetRotation = quaternion.AxisAngle(new float3(0, 1, 0), angleData.TargetAngleY);
                        pivotTransform.Rotation = math.slerp(angleData.InitialRotation, targetRotation, t);
                        UnityEngine.Debug.Log($"[CameraRotationSystem] | pivotTransform.Rotation: {pivotTransform.Rotation}");
                    }
                    else
                    {
                        angleData.AnimationState = AngleAnimationState.Finished;
                        UnityEngine.Debug.Log($"[CameraRotationSystem] | id: {playerID} rotation: {pivotTransform.Rotation}");
                        playerToCameraAngle.Map[playerID] = pivotTransform.Rotation;
                    }
                    break;
            }
        }

        /// <summary>
        /// If the stateful event involves a camera zone output that camera zone's entity.
        /// <param name="statefulEvent"> The trigger event in the buffer. </param>
        /// <param name="entity"> Camera zone entity for the trigger event. </param>
        /// </summary>
        [BurstCompile]
        private bool IsCameraStatefulEvent(ref SystemState _, in StatefulTriggerEvent statefulEvent, out Entity entity)
        {
            if (SystemAPI.HasComponent<CameraSceneData>(statefulEvent.EntityA))
            {
                entity = statefulEvent.EntityA;
                return true;
            }

            if (SystemAPI.HasComponent<CameraSceneData>(statefulEvent.EntityB))
            {
                entity = statefulEvent.EntityB;
                return true;
            }

            entity = Entity.Null;
            return false;
        }
    }

    /// <summary>
    /// Stores the necessary information for pivot angle rotation animation
    /// <param name="TargetAngleY"> Angle in radians to rotate the pivot to.</param>
    /// <param name="AnimationState"> Keep track if the pivot rotation animation is in progress or finished.</param>
    /// <param name="RotationSpeed"> How fast the pivot should rotate.</param>
    /// <param name="CurrentAngleY"> The current angle in radians of the pivot.</param>
    /// <param name="InitialRotation"> The quaternion representing the initialRotation.</param>
    /// <param name="Delta"> The change in the animation transition.</param>
    /// </summary>
    public struct AngleAnimationData : IComponentData
    {
        public float TargetAngleY;
        public AngleAnimationState AnimationState;
        public int RotationSpeed;
        public float CurrentAngleY;
        public quaternion InitialRotation;
        public float Delta;
    }
}

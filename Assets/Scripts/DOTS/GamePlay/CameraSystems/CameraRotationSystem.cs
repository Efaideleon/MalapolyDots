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
                                var angleAnimationData = SystemAPI.GetSingletonRW<AngleAnimationData>();

                                angleAnimationData.ValueRW.TargetAngleY = math.radians(cameraZoneData.RotationAngleY);
                                angleAnimationData.ValueRW.AnimationState = AngleAnimationState.InProgress;
                                angleAnimationData.ValueRW.RotationSpeed = cameraZoneData.RotationSpeed;

                                // TODO: This will depend on the previous state of the camera for the current player.
                                angleAnimationData.ValueRW.CurrentAngleY = 0;
                                break;
                        }
                    }
                }
            }

            AnimateAngle(ref state, ref deltaTime);
        }

        /// <summary>
        /// Animations the angle from the current state
        /// </summary>
        public readonly void AnimateAngle(ref SystemState _, ref float dt)
        {
            var angleData = SystemAPI.GetSingletonRW<AngleAnimationData>();
            switch (angleData.ValueRO.AnimationState)
            {
                case AngleAnimationState.InProgress:
                    var pivotTransform = SystemAPI.GetSingletonRW<PivotTransform>();

                    var newAngleY = angleData.ValueRO.CurrentAngleY + angleData.ValueRO.RotationSpeed * dt;
                    if (newAngleY <= angleData.ValueRO.TargetAngleY)
                    {
                        angleData.ValueRW.CurrentAngleY = newAngleY;
                        pivotTransform.ValueRW.Rotation = quaternion.Euler(0, newAngleY, 0);
                    }
                    else
                    {
                        pivotTransform.ValueRW.Rotation = quaternion.Euler(0, angleData.ValueRO.TargetAngleY, 0);
                        angleData.ValueRW.AnimationState = AngleAnimationState.Finished;
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
    /// </summary>
    public struct AngleAnimationData : IComponentData
    {
        public float TargetAngleY;
        public AngleAnimationState AnimationState;
        public int RotationSpeed;
        public float CurrentAngleY;
    }
}

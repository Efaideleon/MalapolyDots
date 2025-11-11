using DOTS.Characters;
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
    [BurstCompile]
    public partial struct CameraRotationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            var animationEnitity = state.EntityManager.CreateEntity(stackalloc ComponentType[]
            {
                ComponentType.ReadOnly<TargetAngleYComponent>(),
                ComponentType.ReadOnly<AngleAnimationStateComponent>(),
                ComponentType.ReadOnly<RotationSpeedComponent>(),
                ComponentType.ReadOnly<InitialRotationComponent>(),
                ComponentType.ReadOnly<DeltaComponent>(),
            });

            SystemAPI.SetComponent(animationEnitity, new TargetAngleYComponent { });
            SystemAPI.SetComponent(animationEnitity, new AngleAnimationStateComponent { });
            SystemAPI.SetComponent(animationEnitity, new RotationSpeedComponent { });
            SystemAPI.SetComponent(animationEnitity, new InitialRotationComponent { });
            SystemAPI.SetComponent(animationEnitity, new DeltaComponent { });

            state.RequireForUpdate<StatefulTriggerEvent>();
            state.RequireForUpdate<CameraSceneData>();
            state.RequireForUpdate<CurrentPlayerComponent>();
            state.RequireForUpdate<MainCameraTransform>();
            state.RequireForUpdate<PivotRotation>();
            state.RequireForUpdate<PivotTransformTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            var currentPlayer = SystemAPI.GetSingletonRW<CurrentPlayerComponent>();
            if (currentPlayer.ValueRO.entity == Entity.Null || !SystemAPI.Exists(currentPlayer.ValueRO.entity)) return;
            
            var currentPlayerPivotRotation = SystemAPI.GetComponentRW<CurrentPivotRotation>(currentPlayer.ValueRO.entity);
            var pivotEntity = SystemAPI.GetSingletonEntity<PivotTransformTag>();

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

                                foreach (var (targetAngleY, animationState, rotationSpeed, initialRotation, delta)
                                        in SystemAPI.Query<
                                            RefRW<TargetAngleYComponent>,
                                            RefRW<AngleAnimationStateComponent>,
                                            RefRW<RotationSpeedComponent>,
                                            RefRW<InitialRotationComponent>,
                                            RefRW<DeltaComponent>
                                        >())
                                {
                                    targetAngleY.ValueRW.Value = math.radians(cameraZoneData.RotationAngleY);
                                    animationState.ValueRW.Value = AngleAnimationState.InProgress;
                                    rotationSpeed.ValueRW.Value = cameraZoneData.RotationSpeed;
                                    initialRotation.ValueRW.Value = currentPlayerPivotRotation.ValueRO.Value;
                                    delta.ValueRW.Value = 0;
                                }
                                break;
                        }
                    }
                }
            }

            var job = new AnimatePivotJob
            {
                pivotEntity = pivotEntity,
                playerEntity = currentPlayer.ValueRO.entity,
                dt = deltaTime,
                ecb = GetECB(ref state).AsParallelWriter()
            };

            job.ScheduleParallel();
        }

        /// <summary>
        /// Animates the pivot to rotate to an absolute target angle in the world space. 
        /// </summary>
        //[BurstCompile]
        private partial struct AnimatePivotJob : IJobEntity
        {
            /// <summary> The entity for the pivot transform being animated.</summary>
            public Entity pivotEntity;

            /// <summary> The current player's entity.</summary>
            public Entity playerEntity;

            /// <summary> The delta over time to animate the transition between quaternion to another.</summary>
            public float dt;

            /// <summary> A reference to the EndSimulationEntityCommandBuffer as parralel writer.</summary>
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute(
                    [ChunkIndexInQuery] int chunkIndex,
                    ref TargetAngleYComponent targetAngleY,
                    ref AngleAnimationStateComponent animationState,
                    ref RotationSpeedComponent rotationSpeed,
                    ref InitialRotationComponent initialRotation,
                    ref DeltaComponent delta)
            {
                switch (animationState.Value)
                {
                    case AngleAnimationState.InProgress:

                        delta.Value += rotationSpeed.Value * dt;
                        float t = math.clamp(delta.Value, 0, 1);

                        var targetRotation = quaternion.AxisAngle(new float3(0, 1, 0), targetAngleY.Value);

                        var newPivotRotation = new PivotRotation
                        {
                            Value = math.slerp(initialRotation.Value, targetRotation, t),
                        };
                        ecb.SetComponent(chunkIndex, pivotEntity, newPivotRotation);

                        if (t == 1)
                        {
                            ecb.SetComponent(chunkIndex, playerEntity, new CurrentPivotRotation { Value = newPivotRotation.Value });
                            animationState.Value = AngleAnimationState.Finished;
                        }
                        break;
                }
            }
        }

        /// <summary> If the stateful event involves a camera zone output that camera zone's entity.</summary>
        /// <param name="statefulEvent"> The trigger event in the buffer. </param>
        /// <param name="entity"> Camera zone entity for the trigger event. </param>
        /// <returns> True if the state ful events involes a camera zone, false otherswise.</returns>
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

        /// <summary> Entity Commmand Buffer reference. </summary>
        [BurstCompile]
        private EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
        }
    }

    /// <summary> Angle in radians to rotate the pivot to. </summary>
    public struct TargetAngleYComponent : IComponentData
    {
        public float Value;
    }

    /// <summary> Keep track if the pivot rotation animation is in progress or finished. </summary>
    public struct AngleAnimationStateComponent : IComponentData
    {
        public AngleAnimationState Value;
    }

    /// <summary> How fast the pivot should rotate. </summary>
    public struct RotationSpeedComponent : IComponentData
    {
        public int Value;
    }

    /// <summary> The quaternion representing the initialRotation.</summary>
    public struct InitialRotationComponent : IComponentData
    {
        public quaternion Value;
    }

    /// <summary> The change in the animation transition. </summary>
    public struct DeltaComponent : IComponentData
    {
        public float Value;
    }
}

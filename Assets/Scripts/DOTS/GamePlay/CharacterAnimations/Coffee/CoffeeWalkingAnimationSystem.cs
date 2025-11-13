using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Entities;

namespace DOTS.GamePlay.ChanceActionSystems.Coffee
{
    public partial struct CoffeeWalkingAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new WalkingPhaseComponent { WalkingPhase = WalkingPhaseEnum.None });
        }

        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;

            foreach (var (
                        animationNumber,
                        animationState,
                        _) in
                    SystemAPI.Query<
                    RefRW<MaterialOverrideAnimationNumber>,
                    RefRW<CoffeeAnimationStateComponent>,
                    RefRO<CoffeeMaterialTag>
                    >())
            {
                if (animationState.ValueRO.Value == CoffeeAnimationState.Walking)
                {
                    var walkingPhase = SystemAPI.GetSingletonRW<WalkingPhaseComponent>().ValueRW;
                    if (walkingPhase.WalkingPhase == WalkingPhaseEnum.None)
                    {
                        walkingPhase.WalkingPhase = WalkingPhaseEnum.Mounting;
                        var mounting = SystemAPI.GetSingleton<MountingComponent>();
                        animationNumber.ValueRW.Value = mounting.Value;
                    }
                    if (walkingPhase.WalkingPhase == WalkingPhaseEnum.Mounting)
                    {
                        var playMountingJob = new CoffeeMountingAnimationJob { dt = dt };

                        var handle = playMountingJob.Schedule(state.Dependency);
                        handle.Complete();

                        UnityEngine.Debug.Log($"[CoffeeWalkingAnimationSystem] | playing mounting");
                        foreach (var (mountingAnimationState, _) in SystemAPI.Query<RefRO<AnimationStateComponent>, RefRO<MountingComponent>>())
                        {
                            UnityEngine.Debug.Log($"[CoffeeWalkingAnimationSystem] | mountingAnimationState {mountingAnimationState.ValueRO.Value}");
                            if (mountingAnimationState.ValueRO.Value == CharactersAnimationState.Finished)
                            {
                                walkingPhase.WalkingPhase = WalkingPhaseEnum.Walking;
                                var walking = SystemAPI.GetSingleton<WalkingComponent>();
                                animationNumber.ValueRW.Value = walking.Value;
                            }
                        }
                    }
                    if (walkingPhase.WalkingPhase == WalkingPhaseEnum.Walking)
                    {
                        var playWalkingJob = new CoffeeWalkingAnimationJob { dt = dt };
                        var handle =  playWalkingJob.Schedule(state.Dependency);
                        handle.Complete();
                    }
                }
            }
        }
        
        public partial struct CoffeeMountingAnimationJob : IJobEntity
        {
            public float dt;
            public void Execute(
                    MountingFrameRateComponent frameRate,
                    MountingFrameRangeComponent frameRange,
                    MaterialOverride3FrameNumber frame,
                    AnimationStateComponent animationState
                    )
            {
                frame.Value += frameRate.Value * dt; 
                animationState.Value = CharactersAnimationState.Playing;

                if (frame.Value > frameRange.End)
                {
                    animationState.Value = CharactersAnimationState.Finished;
                    frame.Value = frameRange.Start;
                }
            }
        }

        public partial struct CoffeeWalkingAnimationJob : IJobEntity
        {
            public float dt;
            public void Execute(
                    WalkingFrameRateComponent frameRate,
                    WalkingFrameRangeComponent frameRange,
                    MaterialOverride2FrameNumber frame,
                    AnimationStateComponent animationState
                    )
            {
                frame.Value += frameRate.Value * dt; 
                animationState.Value = CharactersAnimationState.Playing;

                if (frame.Value > frameRange.End)
                {
                    animationState.Value = CharactersAnimationState.Finished;
                    frame.Value = frameRange.Start;
                }
            }
        }
    }

    public enum WalkingPhaseEnum
    {
        None,
        Mounting,
        Walking,
        Unmounting
    }

    /// <summary> Stores the current phase in the walking animation. </summary>
    public struct WalkingPhaseComponent : IComponentData
    {
        public WalkingPhaseEnum WalkingPhase;
    }
}

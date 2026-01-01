using Assets.Scripts.DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Burst;
using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.CharacterAnimationSystems2
{
    [BurstCompile]
    public partial struct AnimationFrameAdvancement : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AnimationStateComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            new AnimationFrameAdvancementJob { dt = dt }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct AnimationFrameAdvancementJob : IJobEntity
    {
        public float dt;
        public void Execute(ref AnimationStateComponent animationState, in AnimationDataLibrary library)
        {
            ref var clip = ref library.GetClip(animationState.CurrentAnimation, animationState.Phase);
            animationState.Frame += clip.FrameRate * dt;
        }
    }
}

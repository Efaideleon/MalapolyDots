using Assets.Scripts.DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Assets.Scripts.DOTS.GamePlay.CharacterAnimationSystems2
{
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(AnimationFrameAdvancement))]
    public partial struct FrameSamplingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AnimationStateComponent>();
            state.RequireForUpdate<CurrentFrameVAT>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new FrameSamplingJob { }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct FrameSamplingJob : IJobEntity
    {
        public void Execute(in AnimationStateComponent animationState, in AnimationDataLibrary library, ref CurrentFrameVAT currentFrameVAT)
        {
            ref var clip = ref library.GetClip(animationState.CurrentAnimation, animationState.Phase);
            float frame = math.floor(animationState.Frame);
            frame = math.clamp(frame, clip.FrameRange.Start, clip.FrameRange.End);

            currentFrameVAT.Value = frame;
        }
    }
}

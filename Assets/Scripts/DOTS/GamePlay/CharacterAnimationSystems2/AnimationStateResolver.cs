using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay.CharacterAnimationSystems2
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [BurstCompile]
    [UpdateAfter(typeof(ResolveDesiredAnimation))]
    [UpdateAfter(typeof(AnimationFrameAdvancement))]
    public partial struct AnimationStateResolver : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DesiredAnimation>();
            state.RequireForUpdate<AnimationStateComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new AnimationStateResolverJob { }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct AnimationStateResolverJob : IJobEntity
    {
        public void Execute(in DesiredAnimation desiredAnimation, ref AnimationStateComponent animationState, in AnimationDataLibrary library)
        {
            var desired = desiredAnimation.Value;
            if (desired == CharacterAnimationEnum.None) desired = CharacterAnimationEnum.Idle;
            if (animationState.CurrentAnimation == CharacterAnimationEnum.None)
            {
                animationState.CurrentAnimation = desired;
                EnterPhase(ref animationState, library, AnimationPhase.Start);
            }
            animationState.PendingAnimation = desired == animationState.CurrentAnimation ? CharacterAnimationEnum.None : desired;

            // Missing entry/exit clips are transitions, never poses to sample at frame zero.
            for (int transition = 0; transition < 6; transition++)
            {
                ref var clip = ref library.GetClip(animationState.CurrentAnimation, animationState.Phase);
                bool pending = animationState.PendingAnimation != CharacterAnimationEnum.None;
                if (animationState.Phase == AnimationPhase.Middle && pending)
                {
                    EnterPhase(ref animationState, library, AnimationPhase.End);
                    continue;
                }
                if (clip.HasClip && animationState.Frame < clip.FrameRange.End + 1) return;
                switch (animationState.Phase)
                {
                    case AnimationPhase.Start:
                        EnterPhase(ref animationState, library, AnimationPhase.Middle);
                        break;
                    case AnimationPhase.Middle:
                        if (clip.HasClip && clip.Loops)
                        {
                            float length = Unity.Mathematics.math.max(1, clip.FrameRange.End - clip.FrameRange.Start + 1);
                            animationState.Frame = clip.FrameRange.Start + (animationState.Frame - clip.FrameRange.Start) % length;
                            return;
                        }
                        EnterPhase(ref animationState, library, AnimationPhase.End);
                        break;
                    default:
                        if (pending)
                        {
                            animationState.CurrentAnimation = animationState.PendingAnimation;
                            animationState.PendingAnimation = CharacterAnimationEnum.None;
                            EnterPhase(ref animationState, library, AnimationPhase.Start);
                        }
                        else EnterPhase(ref animationState, library, AnimationPhase.Middle);
                        break;
                }
            }
        }

        public static void EnterPhase(ref AnimationStateComponent a, in AnimationDataLibrary library, AnimationPhase phase)
        {
            a.Phase = phase;
            a.Frame = library.GetClip(a.CurrentAnimation, phase).FrameRange.Start;
        }
    }
}

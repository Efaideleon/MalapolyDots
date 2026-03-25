using Assets.Scripts.DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Burst;
using Unity.Entities;

namespace Assets.Scripts.DOTS.GamePlay.CharacterAnimationSystems2
{
    [BurstCompile]
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
            var desiredAnim = desiredAnimation.Value;
            if (desiredAnim != animationState.CurrentAnimation && desiredAnim != animationState.PendingAnimation)
            {
                animationState.PendingAnimation = desiredAnim;
            }

            ref var clip = ref library.GetClip(animationState.CurrentAnimation, animationState.Phase);

            bool hasPendingAnimation = animationState.PendingAnimation != CharacterAnimationEnum.None;
            bool isPhaseMiddle = animationState.Phase == AnimationPhase.Middle;
            bool isClipFinished = animationState.Frame >= clip.FrameRange.End;

            if (!isClipFinished)
            {
                // If there is a pending animation while in the middle phase, go to the end phase.
                if (hasPendingAnimation && isPhaseMiddle)
                {
                    EnterPhase(ref animationState, in library, AnimationPhase.End);
                    return;
                }
            }
            else
            {
                // If the animation clip reached the end, pick the next phase.
                switch (animationState.Phase)
                {
                    case AnimationPhase.Start:
                        EnterPhase(ref animationState, library, AnimationPhase.Middle);
                        break;
                    case AnimationPhase.Middle:
                        if (clip.Loops)
                        {
                            animationState.Frame = clip.FrameRange.Start;
                        }
                        else
                        {
                            EnterPhase(ref animationState, library, AnimationPhase.End);
                        }
                        break;
                    case AnimationPhase.End:
                        // If there is a pending animation, make it the current.
                        if (hasPendingAnimation)
                        {
                            animationState.CurrentAnimation = animationState.PendingAnimation;
                            animationState.PendingAnimation = CharacterAnimationEnum.None;
                            EnterPhase(ref animationState, library, AnimationPhase.Start);
                        }
                        else
                        {
                            EnterPhase(ref animationState, library, AnimationPhase.Middle);
                        }
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

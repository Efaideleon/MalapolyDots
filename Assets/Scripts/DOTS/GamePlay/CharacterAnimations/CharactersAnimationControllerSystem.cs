using DOTS.Characters;
using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.Characters.CharactersMaterialAuthoring.CharacterTags;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.GamePlay.CharacterAnimations
{
    ///<summary> This system handles the animation for the coffee character by changing a property in the material.</summary>
    [BurstCompile]
    public partial struct CharactersAnimationControllerSystem : ISystem
    {
        private ComponentLookup<PlayerMovementState> playerMovementState;

        public void OnCreate(ref SystemState state)
        {
            playerMovementState = state.GetComponentLookup<PlayerMovementState>(true);

            state.RequireForUpdate<PlayerMovementState>();
            state.RequireForUpdate<CurrentFrameVAT>();
            state.RequireForUpdate<CurrentAnimationData>();
            state.RequireForUpdate<CoffeeMaterialTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            playerMovementState.Update(ref state);

            var animationStateMachineJob = new CoffeeAnimationStateMachineJob { playerMovementState = playerMovementState, };

            animationStateMachineJob.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct CoffeeAnimationStateMachineJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<PlayerMovementState> playerMovementState;

            public void Execute(ref CurrentAnimationData currAnimation, in Parent parent, CoffeeMaterialTag _,
                    ref AnimationPlayState playState, in AnimationDataLibrary animationDataLibrary)
            {
                if (!playerMovementState.HasComponent(parent.Value)) return;

                var moveState = playerMovementState[parent.Value];
                ref var clips = ref animationDataLibrary.AnimationDataBlobRef.Value.Clips;

                var finished = playState.Value == PlayState.Finished;
                var nextAnimation = AnimationsStateMachine.GetCoffeeNextState(currAnimation.Value.AnimationEnum, moveState.Value, finished);
                AnimationsStateMachine.SetAnimation(ref currAnimation, in clips[(int)nextAnimation], ref playState);
            }
        }
    }

    public static class AnimationsStateMachine
    {
        public static Animations GetCoffeeNextState(Animations curr, MoveState moveState, bool finished)
        {
            switch (moveState)
            {
                case MoveState.Idle:
                    if (curr == Animations.Walking) { return Animations.Unmounting; }
                    else if (curr == Animations.Unmounting && finished) { return Animations.Idle; }
                    else if (curr != Animations.Idle && curr != Animations.Unmounting) { return Animations.Idle; }
                    break;
                case MoveState.Walking:
                    if (curr == Animations.Idle) { return Animations.Mounting; }
                    else if (curr == Animations.Mounting && finished) { return Animations.Walking; }
                    else if (curr != Animations.Mounting && curr != Animations.Walking) { return Animations.Walking; }
                    break;
            }
            return curr;
        }

        public static void SetAnimation(ref CurrentAnimationData currAnimation, in AnimationData newAnimation, ref AnimationPlayState playState)
        {
            currAnimation.Value = newAnimation;
            playState.Value = PlayState.Playing;
        }
    }
}

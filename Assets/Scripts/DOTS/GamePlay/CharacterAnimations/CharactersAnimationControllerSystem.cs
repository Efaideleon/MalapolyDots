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
            state.RequireForUpdate<CoffeeMaterialTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            playerMovementState.Update(ref state);

            //var animationStateMachineJob = new CoffeeAnimationStateMachineJob { playerMovementState = playerMovementState };
            //animationStateMachineJob.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct CoffeeAnimationStateMachineJob : IJobEntity
        {
            [ReadOnly] public ComponentLookup<PlayerMovementState> playerMovementState;

            public void Execute(ref CurrentCharacterAnimation currAnimation, in Parent parent, CoffeeMaterialTag _,
                    ref AnimationPlayState playState, in AnimationDataLibrary animationDataLibrary)
            {
                if (!playerMovementState.HasComponent(parent.Value)) return;

                var moveState = playerMovementState[parent.Value];
                ref var clips = ref animationDataLibrary.AnimationDataBlobRef.Value.Clips;

                var finished = playState.Value == PlayState.Finished;
                var nextAnimation = AnimationsStateMachine.GetCoffeeNextState(currAnimation.Value, moveState.Value, finished);
                AnimationsStateMachine.SetAnimation(ref currAnimation, in nextAnimation, ref playState);
            }
        }
    }

    public static class AnimationsStateMachine
    {
        public static CharacterAnimation GetCoffeeNextState(CharacterAnimation curr, MoveState moveState, bool finished)
        {
            switch (moveState)
            {
                case MoveState.Idle:
                    if (curr == CharacterAnimation.Walking) { return CharacterAnimation.Unmounting; }
                    else if (curr == CharacterAnimation.Unmounting && finished) { return CharacterAnimation.Idle; }
                    else if (curr != CharacterAnimation.Idle && curr != CharacterAnimation.Unmounting) { return CharacterAnimation.Idle; }
                    break;
                case MoveState.Walking:
                    if (curr == CharacterAnimation.Idle) { return CharacterAnimation.Mounting; }
                    else if (curr == CharacterAnimation.Mounting && finished) { return CharacterAnimation.Walking; }
                    else if (curr != CharacterAnimation.Mounting && curr != CharacterAnimation.Walking) { return CharacterAnimation.Walking; }
                    break;
            }
            return curr;
        }

        public static void SetAnimation(ref CurrentCharacterAnimation currAnimation, in CharacterAnimation newAnimation, ref AnimationPlayState playState)
        {
            currAnimation.Value = newAnimation;
            playState.Value = PlayState.Playing;
        }
    }
}

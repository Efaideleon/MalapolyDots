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

            var animationStateMachineJob = new CoffeeAnimationStateMachineJob { playerMovementState = playerMovementState };

            animationStateMachineJob.ScheduleParallel();
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
        public static Characters.CharactersMaterialAuthoring.CharacterAnimations GetCoffeeNextState(Characters.CharactersMaterialAuthoring.CharacterAnimations curr, MoveState moveState, bool finished)
        {
            switch (moveState)
            {
                case MoveState.Idle:
                    if (curr == Characters.CharactersMaterialAuthoring.CharacterAnimations.Walking) { return Characters.CharactersMaterialAuthoring.CharacterAnimations.Unmounting; }
                    else if (curr == Characters.CharactersMaterialAuthoring.CharacterAnimations.Unmounting && finished) { return Characters.CharactersMaterialAuthoring.CharacterAnimations.Idle; }
                    else if (curr != Characters.CharactersMaterialAuthoring.CharacterAnimations.Idle && curr != Characters.CharactersMaterialAuthoring.CharacterAnimations.Unmounting) { return Characters.CharactersMaterialAuthoring.CharacterAnimations.Idle; }
                    break;
                case MoveState.Walking:
                    if (curr == Characters.CharactersMaterialAuthoring.CharacterAnimations.Idle) { return Characters.CharactersMaterialAuthoring.CharacterAnimations.Mounting; }
                    else if (curr == Characters.CharactersMaterialAuthoring.CharacterAnimations.Mounting && finished) { return Characters.CharactersMaterialAuthoring.CharacterAnimations.Walking; }
                    else if (curr != Characters.CharactersMaterialAuthoring.CharacterAnimations.Mounting && curr != Characters.CharactersMaterialAuthoring.CharacterAnimations.Walking) { return Characters.CharactersMaterialAuthoring.CharacterAnimations.Walking; }
                    break;
            }
            return curr;
        }

        public static void SetAnimation(ref CurrentCharacterAnimation currAnimation, in Characters.CharactersMaterialAuthoring.CharacterAnimations newAnimation, ref AnimationPlayState playState)
        {
            currAnimation.Value = newAnimation;
            playState.Value = PlayState.Playing;
        }
    }
}

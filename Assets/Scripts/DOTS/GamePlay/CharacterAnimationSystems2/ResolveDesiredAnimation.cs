using Assets.Scripts.DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.Characters;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Assets.Scripts.DOTS.GamePlay.CharacterAnimationSystems2
{
    [BurstCompile]
    public partial struct ResolveDesiredAnimation : ISystem
    {
        public ComponentLookup<PlayerMovementState> moveStateLookup;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DesiredAnimation>();
            state.RequireForUpdate<PlayerMovementState>();
            moveStateLookup = SystemAPI.GetComponentLookup<PlayerMovementState>(isReadOnly: true);
        }

        public void OnUpdate(ref SystemState state)
        {
            moveStateLookup.Update(ref state);
            new ResolveDesiredAnimationJob { moveStateLookup = moveStateLookup }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct ResolveDesiredAnimationJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<PlayerMovementState> moveStateLookup;
        public void Execute(
                ref DesiredAnimation desiredAnimation,
                in Parent parent)
        {
            if (!moveStateLookup.HasComponent(parent.Value))
                return;

            var moveState = moveStateLookup[parent.Value];

            CharacterAnimationEnum desiredState =
                moveState.Value == MoveState.Idle ? CharacterAnimationEnum.Idle :
                moveState.Value == MoveState.Walking ? CharacterAnimationEnum.Walking :
                CharacterAnimationEnum.None;

            if (desiredState != desiredAnimation.Value)
            {
                desiredAnimation.Value = desiredState;
            }
        }
    }
}

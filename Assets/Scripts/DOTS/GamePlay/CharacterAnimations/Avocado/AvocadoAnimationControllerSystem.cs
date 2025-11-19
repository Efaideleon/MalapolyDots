using DOTS.Characters;
using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.Characters.CharactersMaterialAuthoring.CharactersTags;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.GamePlay.CharacterAnimations
{
    ///<summary> This system handles the animation for the coffee character by changing a property in the material.</summary>
    [BurstCompile]
    public partial struct AvocadoAnimationControllerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AnimationNumberMO>();
            state.RequireForUpdate<AvocadoMaterialTag>();
            state.RequireForUpdate<PlayerMovementState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            bool run = false;
            foreach (var _ in SystemAPI.Query<RefRO<AnimationPlayState>, RefRO<AvocadoMaterialTag>>().WithChangeFilter<AnimationPlayState>())
                run = true;

            foreach (var _ in SystemAPI.Query<RefRO<PlayerMovementState>>().WithChangeFilter<PlayerMovementState>())
                run = true;

            if (run)
            {
                foreach (var (animation, parent, _, entity) in
                        SystemAPI.Query<
                        RefRW<CurrentAnimation>,
                        RefRW<Parent>,
                        RefRO<AvocadoMaterialTag>
                        >().WithEntityAccess())
                {
                    if (SystemAPI.HasComponent<PlayerMovementState>(parent.ValueRO.Value))
                    {
                        var moveState = SystemAPI.GetComponent<PlayerMovementState>(parent.ValueRO.Value);

                        switch (moveState.Value)
                        {
                            case MoveState.Idle:
                                animation.ValueRW.Value = Animations.Idle;
                                AnimationTagSwitcher(ref state, in entity, Animations.Idle);
                                break;
                            case MoveState.Walking:
                                animation.ValueRW.Value = Animations.Walking;
                                AnimationTagSwitcher(ref state, in entity, Animations.Walking);
                                break;
                        }
                    }
                }
            }
        }

        public void AnimationTagSwitcher(ref SystemState _, in Entity entity, Animations animation)
        {
            SystemAPI.SetComponentEnabled<IdleAnimationTag>(entity, animation == Animations.Idle);
            SystemAPI.SetComponentEnabled<WalkingAnimationTag>(entity, animation == Animations.Walking);
        }
    }
}

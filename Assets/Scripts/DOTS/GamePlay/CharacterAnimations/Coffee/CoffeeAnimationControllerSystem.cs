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
    public partial struct CoffeeAnimationControllerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AnimationNumberMO>();
            state.RequireForUpdate<CoffeeMaterialTag>();
            state.RequireForUpdate<PlayerMovementState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            bool run = false;
            foreach (var _ in SystemAPI.Query<RefRO<AnimationPlayState>, RefRO<CoffeeMaterialTag>>().WithChangeFilter<AnimationPlayState>())
            {
                run = true;
            }

            foreach (var _ in SystemAPI.Query<RefRO<PlayerMovementState>>().WithChangeFilter<PlayerMovementState>())
            {
                run = true;
            }

            if (run)
            {
                foreach (var (animation, parent, _, entity) in
                        SystemAPI.Query<
                        RefRW<CurrentAnimation>,
                        RefRW<Parent>,
                        RefRO<CoffeeMaterialTag>
                        >().WithEntityAccess())
                {
                    if (SystemAPI.HasComponent<PlayerMovementState>(parent.ValueRO.Value))
                    {
                        var moveState = SystemAPI.GetComponent<PlayerMovementState>(parent.ValueRO.Value);

                        switch (moveState.Value)
                        {
                            case MoveState.Idle:
                                switch (animation.ValueRO.Value)
                                {
                                    case Animations.Walking:
                                        animation.ValueRW.Value = Animations.Unmounting;
                                        AnimationTagSwitcher(ref state, in entity,Animations.Unmounting);
                                        break;
                                    default: 
                                        animation.ValueRW.Value = Animations.Idle;
                                        AnimationTagSwitcher(ref state, in entity,Animations.Idle);
                                        break;
                                }
                                break;
                            case MoveState.Walking:
                                switch (animation.ValueRO.Value)
                                {
                                    case Animations.Idle:
                                        animation.ValueRW.Value = Animations.Mounting;
                                        AnimationTagSwitcher(ref state, in entity,Animations.Mounting);
                                        break;
                                    default: 
                                        animation.ValueRW.Value = Animations.Walking;
                                        AnimationTagSwitcher(ref state, in entity,Animations.Walking);
                                        break;
                                }
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
            SystemAPI.SetComponentEnabled<MountingAnimationTag>(entity, animation == Animations.Mounting);
            SystemAPI.SetComponentEnabled<UnmountingAnimationTag>(entity, animation == Animations.Unmounting);
        }
    }
}

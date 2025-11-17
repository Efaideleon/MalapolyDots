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
                run = true;

            foreach (var _ in SystemAPI.Query<RefRO<PlayerMovementState>>().WithChangeFilter<PlayerMovementState>())
                run = true;

            if (run)
            {
                foreach (var (animation, parent, _) in
                        SystemAPI.Query<
                        RefRW<CurrentAnimation>,
                        RefRW<Parent>,
                        RefRO<CoffeeMaterialTag>
                        >())
                {
                    if (SystemAPI.HasComponent<PlayerMovementState>(parent.ValueRO.Value))
                    {
                        var moveState = SystemAPI.GetComponent<PlayerMovementState>(parent.ValueRO.Value);

                        switch (moveState.Value)
                        {
                            case MoveState.Idle:
                                animation.ValueRW.Value = animation.ValueRO.Value switch
                                {
                                    Animations.Walking => Animations.Unmounting,
                                    _ => Animations.Idle,
                                };
                                break;
                            case MoveState.Walking:
                                animation.ValueRW.Value = animation.ValueRO.Value switch
                                {
                                    Animations.Idle => Animations.Mounting,
                                    _ => Animations.Walking,
                                };
                                break;
                        }
                    }
                }
            }
        }
    }
}

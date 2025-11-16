using DOTS.Characters;
using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.GamePlay.CharacterAnimations
{
    ///<summary> This system handles the animation for the coffee character by changing a property in the material.</summary>
    public partial struct CoffeeAnimationControllerSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new CoffeeAnimationControllerPlayState { Value = PlayState.NotPlaying });
            state.RequireForUpdate<MaterialOverrideAnimationNumber>();
            state.RequireForUpdate<CoffeeMaterialTag>();
            state.RequireForUpdate<PlayerMovementState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            bool run = false;
            foreach (var _ in SystemAPI.Query<RefRO<CoffeeAnimationControllerPlayState>>().WithChangeFilter<CoffeeAnimationControllerPlayState>())
                run = true;

            foreach (var _ in SystemAPI.Query<RefRO<PlayerMovementState>>().WithChangeFilter<PlayerMovementState>())
                run = true;

            if (run)
            {
                foreach (var (animation, parent, _) in
                        SystemAPI.Query<
                        RefRW<CoffeeAnimationComponent>,
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
                                    CoffeeAnimation.Walking => CoffeeAnimation.Unmounting,
                                    _ => CoffeeAnimation.Idle,
                                };
                                break;
                            case MoveState.Walking:
                                animation.ValueRW.Value = animation.ValueRO.Value switch
                                {
                                    CoffeeAnimation.Idle => CoffeeAnimation.Mounting,
                                    _ => CoffeeAnimation.Walking,
                                };
                                break;
                        }
                    }
                }
            }
        }
    }

    /// <summary> Tracks if an animation is playing for the coffee </summary>
    public struct CoffeeAnimationControllerPlayState : IComponentData
    {
        public PlayState Value;
    }
}

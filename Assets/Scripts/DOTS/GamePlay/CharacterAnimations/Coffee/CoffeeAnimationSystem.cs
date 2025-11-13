using DOTS.Characters;
using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.GamePlay.CharacterAnimations
{
    ///<summary> This system handles the animation for the avocado character by changing a property in the material.</summary>
    public partial struct CoffeeAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MaterialOverrideAnimationNumber>();
            state.RequireForUpdate<CoffeeMaterialTag>();
            state.RequireForUpdate<PlayerMovementState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var _ in SystemAPI.Query<RefRO<PlayerMovementState>>().WithChangeFilter<PlayerMovementState>())
            {
                foreach (var (animationState, parent, _) in
                        SystemAPI.Query<
                        RefRW<CoffeeAnimationStateComponent>,
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
                                animationState.ValueRW.Value = CoffeeAnimationState.Idle;
                                break;
                            case MoveState.Walking:
                                animationState.ValueRW.Value = CoffeeAnimationState.Walking;
                                break;
                        }
                    }
                }
            }
        }
    }
}

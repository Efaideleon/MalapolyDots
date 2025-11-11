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
                foreach (var (IDLE, WALKING, animationNumber, parent, _) in
                        SystemAPI.Query<
                        RefRO<IdleComponent>,
                        RefRO<WalkingComponent>,
                        RefRW<MaterialOverrideAnimationNumber>,
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
                                animationNumber.ValueRW.Value = IDLE.ValueRO.Value;
                                break;
                            case MoveState.Walking:
                                animationNumber.ValueRW.Value = WALKING.ValueRO.Value;
                                break;
                        }
                    }
                }
            }

            var entity = SystemAPI.GetSingletonEntity<CoffeeMaterialTag>();
            var idle = SystemAPI.GetComponentRO<IdleComponent>(entity);
            var frame = SystemAPI.GetComponentRW<MaterialOverrideFrameNumber>(entity);
            var animationNum = SystemAPI.GetComponentRO<MaterialOverrideAnimationNumber>(entity);

            if (animationNum.ValueRO.Value == idle.ValueRO.Value)
            {
                frame.ValueRW.Value += 1; 
                if (frame.ValueRO.Value == 39)
                {
                    frame.ValueRW.Value = 1;
                }
            }
        }
    }

    /// <summary> The current animation number for coffee. </summary>
    public struct CoffeeAnimationNumber : IComponentData
    {
        public float Value;
    }
}

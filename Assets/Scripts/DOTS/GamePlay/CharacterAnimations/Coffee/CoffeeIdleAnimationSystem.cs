using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Entities;

namespace DOTS.GamePlay.CharacterAnimations.Coffee
{
    /// <summary>Plays the coffee idle animation by changing the frame number. </summary>
    public partial struct CoffeeIdleAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CoffeeMaterialTag>();
            state.RequireForUpdate<CoffeeAnimationControllerPlayState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            foreach (var (IDLE, idleFrameRate, frameRange, animation, animationNumber, frameNumber, animationPlayState) in
                    SystemAPI.Query<
                    RefRO<IdleAnimationNumber>,
                    RefRO<IdleFrameRateComponent>,
                    RefRO<IdleFrameRangeComponent>,
                    RefRW<CoffeeAnimationComponent>,
                    RefRW<MaterialOverrideAnimationNumber>,
                    RefRW<MaterialOverride1FrameNumber>,
                    RefRW<CoffeeAnimationPlayState>
                    >())
            {
                if (animation.ValueRO.Value == CoffeeAnimation.Idle)
                {
                    animationNumber.ValueRW.Value = IDLE.ValueRO.Value; 
                    frameNumber.ValueRW.Value += idleFrameRate.ValueRO.Value * dt ;

                    if (animationPlayState.ValueRO.Value != PlayState.Playing)
                    {
                        animationPlayState.ValueRW.Value = PlayState.Playing;
                        SystemAPI.GetSingletonRW<CoffeeAnimationControllerPlayState>().ValueRW.Value = PlayState.Playing;
                    }

                    if (frameNumber.ValueRO.Value > frameRange.ValueRO.End)
                    {
                        frameNumber.ValueRW.Value = frameRange.ValueRO.Start;
                    }
                }
            }
        }
    }
}

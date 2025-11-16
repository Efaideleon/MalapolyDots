using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Entities;

namespace DOTS.GamePlay.CharacterAnimations.Coffee
{
    /// <summary>Plays the coffee unmounting animation by changing the frame number. </summary>
    public partial struct CoffeeUnmountingAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CoffeeMaterialTag>();
            state.RequireForUpdate<CoffeeAnimationControllerPlayState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            foreach (var (unmounting, frameRate, frameRange, animation, animationNumber, frameNumber, animationPlayState) in
                    SystemAPI.Query<
                    RefRO<UnmountingAnimationNumber>,
                    RefRO<UnmountingFrameRateComponent>,
                    RefRO<UnmountingFrameRangeComponent>,
                    RefRW<CoffeeAnimationComponent>,
                    RefRW<MaterialOverrideAnimationNumber>,
                    RefRW<MaterialOverride4FrameNumber>,
                    RefRW<CoffeeAnimationPlayState>
                    >())
            {
                if (animation.ValueRO.Value == CoffeeAnimation.Unmounting)
                {
                    animationNumber.ValueRW.Value = unmounting.ValueRO.Value; 
                    frameNumber.ValueRW.Value += frameRate.ValueRO.Value * dt ;

                    if (animationPlayState.ValueRO.Value != PlayState.Playing)
                    {
                        animationPlayState.ValueRW.Value = PlayState.Playing;
                        SystemAPI.GetSingletonRW<CoffeeAnimationControllerPlayState>().ValueRW.Value = PlayState.Playing;
                    }

                    if (frameNumber.ValueRO.Value > frameRange.ValueRO.End)
                    {
                        frameNumber.ValueRW.Value = frameRange.ValueRO.Start;
                        animationPlayState.ValueRW.Value = PlayState.Finished;
                        SystemAPI.GetSingletonRW<CoffeeAnimationControllerPlayState>().ValueRW.Value = PlayState.Finished;
                    }
                }
            }
        }
    }
}


using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Entities;

namespace DOTS.GamePlay.CharacterAnimations.Coffee
{
    /// <summary>Plays the coffee mounting animation by changing the frame number. </summary>
    public partial struct CoffeeMountingAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CoffeeMaterialTag>();
            state.RequireForUpdate<CoffeeAnimationControllerPlayState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            foreach (var (mounting, frameRate, frameRange, animation, animationNumber, frameNumber, animationPlayState) in
                    SystemAPI.Query<
                    RefRO<MountingAnimationNumber>,
                    RefRO<MountingFrameRateComponent>,
                    RefRO<MountingFrameRangeComponent>,
                    RefRW<CoffeeAnimationComponent>,
                    RefRW<MaterialOverrideAnimationNumber>,
                    RefRW<MaterialOverride3FrameNumber>,
                    RefRW<CoffeeAnimationPlayState>
                    >())
            {
                if (animation.ValueRO.Value == CoffeeAnimation.Mounting)
                {
                    animationNumber.ValueRW.Value = mounting.ValueRO.Value; 
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

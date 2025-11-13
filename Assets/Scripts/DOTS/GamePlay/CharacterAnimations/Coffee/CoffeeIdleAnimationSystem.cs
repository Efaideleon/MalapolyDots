using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Entities;

namespace DOTS.GamePlay.CharacterAnimations.Coffee
{
    public partial struct CoffeeIdleAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {}

        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            foreach (var (IDLE, idleFrameRate, frameRange, animationState, animationNumber, frameNumber, _) in
                    SystemAPI.Query<
                    RefRO<IdleComponent>,
                    RefRO<IdleFrameRateComponent>,
                    RefRO<IdleFrameRangeComponent>,
                    RefRW<CoffeeAnimationStateComponent>,
                    RefRW<MaterialOverrideAnimationNumber>,
                    RefRW<MaterialOverride1FrameNumber>,
                    RefRO<CoffeeMaterialTag>
                    >())
            {
                if (animationState.ValueRO.Value == CoffeeAnimationState.Idle)
                {
                    animationNumber.ValueRW.Value = IDLE.ValueRO.Value; // TODO: Maybe move this out of this loop?
                    frameNumber.ValueRW.Value += idleFrameRate.ValueRO.Value * dt ;
                    if (frameNumber.ValueRO.Value > frameRange.ValueRO.End)
                    {
                        frameNumber.ValueRW.Value = frameRange.ValueRO.Start;
                    }
                }
            }
        }
    }
}

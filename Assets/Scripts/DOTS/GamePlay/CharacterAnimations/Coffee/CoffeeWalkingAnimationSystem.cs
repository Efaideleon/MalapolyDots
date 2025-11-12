using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Entities;

namespace DOTS.GamePlay.CharacterAnimations.Coffee
{
    public partial struct CoffeeWalkingAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {}

        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            foreach (var (IDLE, idleFrameRate, frameRange, animationNumber, frameNumber, _) in
                    SystemAPI.Query<
                    RefRO<IdleComponent>,
                    RefRO<IdleFrameRateComponent>,
                    RefRO<IdleFrameRangeComponent>,
                    RefRW<MaterialOverrideAnimationNumber>,
                    RefRW<MaterialOverrideFrameNumber>,
                    RefRO<CoffeeMaterialTag>
                    >())
            {
                if (animationNumber.ValueRO.Value == IDLE.ValueRO.Value)
                {
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

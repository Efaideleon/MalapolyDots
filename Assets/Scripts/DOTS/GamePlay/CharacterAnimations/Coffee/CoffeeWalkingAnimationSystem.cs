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
            foreach (var (IDLE, WALKING, animationNumber, frameNumber, _) in
                    SystemAPI.Query<
                    RefRO<IdleComponent>,
                    RefRO<WalkingComponent>,
                    RefRW<MaterialOverrideAnimationNumber>,
                    RefRW<MaterialOverrideFrameNumber>,
                    RefRO<CoffeeMaterialTag>
                    >())
            {
                if (animationNumber.ValueRO.Value == IDLE.ValueRO.Value)
                {
                    frameNumber.ValueRW.Value += 60 * dt ;
                    if (frameNumber.ValueRO.Value > 39)
                    {
                        frameNumber.ValueRW.Value = 1;
                    }
                }
            }
        }
    }
}

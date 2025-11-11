using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.GamePlay.CharacterAnimations.Coffee
{
    public partial struct CoffeeWalkingAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {}

        public void OnUpdate(ref SystemState state)
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
                if (animationNumber.ValueRO.Value == IDLE.ValueRO.Value)
                {}
            }
        }
    }
}

using DOTS.Characters;
using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.GamePlay.CharacterAnimations
{
    ///<summary> This system handles the animation for the coffee character by changing a property in the material.</summary>
    [BurstCompile]
    public partial struct CoffeeAnimationControllerSystem : ISystem
    {
        // TODO: Use a component lookup system for the movestate.
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerMovementState>();
            state.RequireForUpdate<CurrentFrameVAT>();
            state.RequireForUpdate<CurrentAnimationData>();
            state.RequireForUpdate<CoffeeMaterialTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            bool run = false;
            foreach (var _ in SystemAPI.Query<RefRO<AnimationPlayState>, RefRO<CoffeeMaterialTag>>().WithChangeFilter<AnimationPlayState>())
            {
                run = true;
            }

            foreach (var _ in SystemAPI.Query<RefRO<PlayerMovementState>>().WithChangeFilter<PlayerMovementState>())
            {
                run = true;
            }

            if (run)
            {
                foreach (var (idle, walking, mounting, unmounting, currAnimationData, currAnimationID, parent) in
                        SystemAPI.Query<
                        RefRO<IdleAnimationData>,
                        RefRO<WalkingAnimationData>,
                        RefRO<MountingAnimationData>,
                        RefRO<UnmountingAnimationData>,
                        RefRW<CurrentAnimationData>,
                        RefRW<CurrentAnimationID>,
                        RefRW<Parent>
                        >()
                        .WithAll<CoffeeMaterialTag>())
                {
                    if (SystemAPI.HasComponent<PlayerMovementState>(parent.ValueRO.Value))
                    {
                        var moveState = SystemAPI.GetComponent<PlayerMovementState>(parent.ValueRO.Value);

                        switch (moveState.Value)
                        {
                            case MoveState.Idle:
                                switch (currAnimationID.ValueRO.Value)
                                {
                                    case Animations.Walking:
                                        currAnimationData.ValueRW.Value = unmounting.ValueRO.Value;
                                        currAnimationID.ValueRW.Value = Animations.Unmounting;
                                        break;
                                    default: 
                                        currAnimationData.ValueRW.Value = idle.ValueRO.Value;
                                        currAnimationID.ValueRW.Value = Animations.Idle;
                                        break;
                                }
                                break;
                            case MoveState.Walking:
                                switch (currAnimationID.ValueRO.Value)
                                {
                                    case Animations.Idle:
                                        currAnimationData.ValueRW.Value = mounting.ValueRO.Value;
                                        currAnimationID.ValueRW.Value = Animations.Mounting;
                                        break;
                                    default: 
                                        currAnimationData.ValueRW.Value = walking.ValueRO.Value;
                                        currAnimationID.ValueRW.Value = Animations.Walking;
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}

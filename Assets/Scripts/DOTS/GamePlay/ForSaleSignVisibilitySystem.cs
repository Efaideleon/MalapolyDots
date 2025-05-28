using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    public partial struct ForSaleSignVisibilitySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ForSaleSignTag>();
            state.RequireForUpdate<VisibleStateComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (visiblityState, _, entity) in 
                    SystemAPI.Query<
                        RefRW<VisibleStateComponent>,
                        RefRO<ForSaleSignTag>
                    >()
                    .WithChangeFilter<VisibleStateComponent>()
                    .WithEntityAccess())
            {
                if (visiblityState.ValueRO.Value == VisibleState.Hidden)
                {
                    if (SystemAPI.HasBuffer<Child>(entity))
                    {
                        var children = SystemAPI.GetBuffer<Child>(entity);

                        // The entity is show the logic side, the child shows the redner side.
                        foreach (var child in children)
                        {
                            var animatorRef = SystemAPI.ManagedAPI.GetComponent<AnimatorReference>(child.Value);
                            //animatorRef.Animator.Set();
                        }
                    }
                }
            }
        }
    }
}

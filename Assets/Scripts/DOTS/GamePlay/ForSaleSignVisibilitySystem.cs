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
            float dt = SystemAPI.Time.DeltaTime;
            var job = new HideForSaleSignJob{ dt = dt };
            job.Schedule(state.Dependency);
        }

        public partial struct HideForSaleSignJob : IJobEntity
        {
            public float dt;

            public void Execute (
                    Entity entity,
                    ref VisibleStateComponent visibleState,
                    ref MaterialOverrideFrame frame,
                    //DynamicBuffer<Child> children,
                    in ForSaleSignTag _
                    )
            {
                if (visibleState.Value == VisibleState.Hidden)
                {
                    frame.Value += dt * 1;
                    // The entity is show the logic side, the child shows the redner side.
                    // foreach (var child in children)
                    // {
                    //     var animatorRef = SystemAPI.ManagedAPI.GetComponent<AnimatorReference>(child.Value);
                    //     //animatorRef.Animator.Set();
                    // }
                    // }
                }
            }
        }
    }
}

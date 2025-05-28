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
            var handle = job.Schedule(state.Dependency);
            handle.Complete();
        }

        public partial struct HideForSaleSignJob : IJobEntity
        {
            public float dt;

            public void Execute (
                    Entity entity,
                    ref VisibleStateComponent visibleState,
                    //DynamicBuffer<Child> children,
                    ref MaterialOverrideFrame frame,
                    in ForSaleSignTag _
                )
            {
                if (visibleState.Value == VisibleState.Hidden)
                {
                    //The entity is show the logic side, the child shows the redner side.
                    // var renderedEntity = children [0];

                    frame.Value += dt * 1;
                }
            }
        }
    }
}

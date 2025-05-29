using DOTS.GameSpaces;
using Unity.Entities;
using Unity.Rendering;

namespace DOTS.GamePlay
{
    public partial struct ForSaleSignVisibilitySystem : ISystem
    {
        private const float AnimationSpeed = 60f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ForSaleSignTag>();
            state.RequireForUpdate<VisibleStateComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            var job = new HideForSaleSignJob
            { 
                dt = dt,
                speed = AnimationSpeed,
                ecb = GetECB(ref state).AsParallelWriter()
            };
            var handle = job.Schedule(state.Dependency);
            handle.Complete();
        }

        public partial struct HideForSaleSignJob : IJobEntity
        {
            public float dt;
            public float speed;
            public EntityCommandBuffer.ParallelWriter ecb; 

            public void Execute (
                    Entity entity,
                    [EntityIndexInQuery] int entityInQueryIndex,
                    ref VisibleStateComponent visibleState,
                    ref MaterialOverrideFrame frame,
                    in ForSaleSignTag _
            )
            {
                if (visibleState.Value == VisibleState.Hiding)
                {
                    frame.Value += dt * speed;
                }
                if (frame.Value >= 59)
                {
                    visibleState.Value = VisibleState.Hidden;

                    // Hides the ForSaleSign when the animation ends
                    ecb.AddComponent<DisableRendering>(entityInQueryIndex, entity);
                }
            }
        }

        public readonly EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecb.CreateCommandBuffer(state.WorldUnmanaged);
        }
    }
}

using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct ForSaleSignVisibilitySystem : ISystem
    {
        private const float HeighThreshold = 10f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ForSaleSignTag>();
            state.RequireForUpdate<VisibleStateComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;
            var job = new HideForSaleSignJob
            {
                dt = dt,
                ecb = GetECB(ref state).AsParallelWriter()
            };
            var handle = job.Schedule(state.Dependency);
            handle.Complete();
        }

        [BurstCompile]
        public partial struct HideForSaleSignJob : IJobEntity
        {
            public float dt;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute(
                    ref LocalTransform localTransform,
                    ref VisibleStateComponent visibleState,
                    in ForSaleSignTag _
            )
            {
                var speed = 5;
                if (visibleState.Value == VisibleState.Hiding)
                {
                    localTransform.Position.y += dt * speed;
                }
                if (localTransform.Position.y >= 59)
                {
                    visibleState.Value = VisibleState.Hidden;

                    // Hides the ForSaleSign when the animation ends
                    // ecb.AddComponent<DisableRendering>(entityInQueryIndex, entity);
                }
            }
        }

        [BurstCompile]
        public readonly EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecb.CreateCommandBuffer(state.WorldUnmanaged);
        }
    }
}

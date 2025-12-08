using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
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
            new HideForSaleSignJob
            {
                dt = dt,
                ecb = GetECB(ref state).AsParallelWriter()
            }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct HideForSaleSignJob : IJobEntity
        {
            public float dt;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute(
                    Entity entity,
                    [EntityIndexInQuery] int entityIndexInQuery,
                    ref PostTransformMatrix postTransformMatrix,
                    ref VisibleStateComponent visibleState,
                    in ForSaleSignTag _)
            {
                if (visibleState.Value == VisibleState.Hiding)
                {
                    float speed = 0.7f;
                    var newY = math.max(0f, postTransformMatrix.Value.c1.y - dt * speed);
                    var newScale = new float3(1, newY, 1);
                    postTransformMatrix.Value = float4x4.Scale(newScale);
                }

                if (math.length(postTransformMatrix.Value.c1.y) <= 0.01f)
                {
                    visibleState.Value = VisibleState.Hidden;
                    ecb.AddComponent<DisableRendering>(entityIndexInQuery, entity);
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

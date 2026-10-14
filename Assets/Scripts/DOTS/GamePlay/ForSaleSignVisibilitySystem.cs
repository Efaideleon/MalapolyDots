using Malapoly.DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
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
                    ref ClientVisibleState clientVisibility,
                    in ForSaleSignTag _)
            {
                const float speed = 0.7f;
                float scaleY = postTransformMatrix.Value.c1.y;

                if (visibleState.Value == VisibleState.Hiding && clientVisibility.State != VisibleState.Hidden)
                {
                    scaleY = math.max(0f, scaleY - dt * speed);
                    postTransformMatrix.Value = float4x4.Scale(new float3(1, scaleY, 1));
                    if (scaleY <= 0.01f)
                    {
                        clientVisibility.State = VisibleState.Hidden;
                        ecb.AddComponent<DisableRendering>(entityIndexInQuery, entity);
                    }
                }
                else if (visibleState.Value == VisibleState.Visible && clientVisibility.State != VisibleState.Visible)
                {
                    if (clientVisibility.State == VisibleState.Hidden)
                        ecb.RemoveComponent<DisableRendering>(entityIndexInQuery, entity);

                    clientVisibility.State = VisibleState.Showing;
                    scaleY = math.min(1f, scaleY + dt * speed);
                    postTransformMatrix.Value = float4x4.Scale(new float3(1, scaleY, 1));
                    if (scaleY >= 0.99f)
                    {
                        postTransformMatrix.Value = float4x4.Scale(new float3(1, 1, 1));
                        clientVisibility.State = VisibleState.Visible;
                    }
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

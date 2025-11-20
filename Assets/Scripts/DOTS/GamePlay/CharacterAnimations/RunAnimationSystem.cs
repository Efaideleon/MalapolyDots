using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay.CharacterAnimations
{
    //[BurstCompile]
    public partial struct RunAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CurrentFrameVAT>();
            state.RequireForUpdate<CurrentAnimationData>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            var animationJob = new CharacterAnimationJob { dt = dt, ecb = GetECB(ref state).AsParallelWriter() };
            animationJob.ScheduleParallel();
        }

        //[BurstCompile]
        public partial struct CharacterAnimationJob : IJobEntity
        {
            private bool finished;
            public float dt;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in CurrentAnimationData data, ref CurrentFrameVAT frame)
            {
                RunAnimation(ref frame.Value, ref finished, data.Value, in dt, loops: false);
                if (finished)
                {
                    ecb.SetComponent(chunkIndex, entity, new AnimationPlayState { Value = PlayState.Finished });
                }
            }
        }

        [BurstCompile]
        public readonly EntityCommandBuffer GetECB(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            return ecb.CreateCommandBuffer(state.WorldUnmanaged);
        }

        //[BurstCompile]
        public static void RunAnimation( ref float frame, ref bool finished, in AnimationData data, in float dt, bool loops)
        {
            frame += data.FrameRate * dt;
            UnityEngine.Debug.Log("running animation");
            UnityEngine.Debug.Log($"running animation frame: {frame}");

            if (frame > data.FrameRange.End)
            {
                frame = data.FrameRange.Start;
                if (!loops) finished = true;
            }
        }
    }
}

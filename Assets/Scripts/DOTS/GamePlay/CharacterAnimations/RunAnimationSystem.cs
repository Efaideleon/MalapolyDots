using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.GamePlay.CharacterAnimations
{
    [BurstCompile]
    public partial struct RunAnimationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<IdleFrameNumberMO>();
            state.RequireForUpdate<WalkingFrameNumberMO>();
            state.RequireForUpdate<MountingFrameNumberMO>();
            state.RequireForUpdate<UnmountingFrameNumberMO>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            var idleJob = new IdleAnimationJob { dt = dt };
            idleJob.ScheduleParallel();

            var walkingJob = new WalkingAnimationJob { dt = dt };
            walkingJob.ScheduleParallel();
            var mountingJob = new MountingAnimationJob { dt = dt, ecb = GetECB(ref state).AsParallelWriter() };
            mountingJob.ScheduleParallel();
            var unmountingJob = new UnmountingAnimationJob { dt = dt, ecb = GetECB(ref state).AsParallelWriter() };
            unmountingJob.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct IdleAnimationJob : IJobEntity
        {
            private bool finished;
            public float dt;

            public void Execute(in IdleAnimationData data, in CurrentAnimation currentAnimation,
                    ref AnimationNumberMO animationNumber, ref IdleFrameNumberMO frame)
            {
                if (currentAnimation.Value == Animations.Idle)
                {
                    AnimationHelper.RunAnimation(ref animationNumber.Value, ref frame.Value, ref finished, data.Data, in dt, loops: true);
                }
            }
        }

        [BurstCompile]
        public partial struct WalkingAnimationJob : IJobEntity
        {
            private bool finished;
            public float dt;

            public void Execute(in WalkingAnimationData data, in CurrentAnimation currentAnimation,
                    ref AnimationNumberMO animationNumber, ref WalkingFrameNumberMO frame)
            {
                if (currentAnimation.Value == Animations.Walking)
                {
                    AnimationHelper.RunAnimation(ref animationNumber.Value, ref frame.Value, ref finished, data.Data, in dt, loops: true);
                }
            }
        }

        [BurstCompile]
        public partial struct MountingAnimationJob : IJobEntity
        {
            private bool finished;
            public float dt;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in MountingAnimationData data, in CurrentAnimation currentAnimation,
                    ref AnimationNumberMO animationNumber, ref MountingFrameNumberMO frame)
            {
                if (currentAnimation.Value == Animations.Mounting)
                {
                    AnimationHelper.RunAnimation(ref animationNumber.Value, ref frame.Value, ref finished, data.Data, in dt, loops: false);
                    if (finished)
                    {
                        ecb.SetComponent(chunkIndex, entity, new AnimationPlayState { Value = PlayState.Finished });
                    }
                }
            }
        }

        [BurstCompile]
        public partial struct UnmountingAnimationJob : IJobEntity
        {
            private bool finished;
            public float dt;
            public EntityCommandBuffer.ParallelWriter ecb;

            public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in UnmountingAnimationData data, in CurrentAnimation currentAnimation,
                    ref AnimationNumberMO animationNumber, ref UnmountingFrameNumberMO frame)
            {
                if (currentAnimation.Value == Animations.Unmounting)
                {
                    AnimationHelper.RunAnimation(ref animationNumber.Value, ref frame.Value, ref finished, data.Data, in dt, loops: false);
                    if (finished)
                    {
                        ecb.SetComponent(chunkIndex, entity, new AnimationPlayState { Value = PlayState.Finished });
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

    public static class AnimationHelper
    {
        public static void RunAnimation(
                ref float animationNumber,
                ref float frame,
                ref bool finished,
                in AnimationData data,
                in float dt,
                bool loops)
        {
            animationNumber = data.Number;
            frame += data.FrameRate * dt;

            if (frame > data.FrameRange.End)
            {
                frame = data.FrameRange.Start;
                if (!loops) finished = true;
            }
        }
    }
}

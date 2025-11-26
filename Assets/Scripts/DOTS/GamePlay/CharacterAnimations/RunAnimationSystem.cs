using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTS.GamePlay.CharacterAnimations
{
    [BurstCompile]
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

            var animationJob = new CharacterAnimationJob { dt = dt };
            animationJob.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct CharacterAnimationJob : IJobEntity
        {
            public float dt;

            public void Execute(CurrentAnimationData data, ref CurrentFrameVAT frame, ref AnimationPlayState playState)
            {
                if (playState.Value == PlayState.Finished) return;
                if (PlayAnimation(ref frame.Value, data.Value, in dt))
                {
                    playState.Value = PlayState.Finished;
                }
            }
        }

        public static bool PlayAnimation(ref float frame, in AnimationData data, in float dt)
        {
            frame = math.clamp(frame + data.FrameRate * dt, data.FrameRange.Start, data.FrameRange.End);
            if (frame >= data.FrameRange.End)
            {
                frame = data.FrameRange.Start;
                if (!data.Loops) return true;
            }
            return false;
        }
    }
}

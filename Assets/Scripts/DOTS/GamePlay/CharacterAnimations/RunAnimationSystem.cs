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
            state.RequireForUpdate<CurrentTreasureFrameVAT>();
        }

        public void OnUpdate(ref SystemState state)
        {
            float dt = SystemAPI.Time.DeltaTime;

            //new CharacterAnimationJob { dt = dt }.ScheduleParallel();
            new TreasureAnimationJob { dt = dt }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct CharacterAnimationJob : IJobEntity
        {
            public float dt;

            public void Execute(CurrentCharacterAnimation animation, in AnimationDataLibrary animationLibrary,
                    ref CurrentFrameVAT frame, ref AnimationPlayState playState)
            {
                if (playState.Value == PlayState.Finished) return;
                ref var clips = ref animationLibrary.AnimationDataBlobRef.Value.Clips;

                // ?????
                // if ((int)CharacterAnimation.Idle >= clips.Length) return;
                //
                // if (PlayAnimation(ref frame.Value, clips[(int)animation.Value], in dt, resets: true, PlayDirection.Forward))
                // {
                //     playState.Value = PlayState.Finished;
                // }
            }
        }

        [BurstCompile]
        public partial struct TreasureAnimationJob : IJobEntity
        {
            public float dt;

            public void Execute(CurrentTreasureAnimation animation, in AnimationDataLibrary animationLibrary,
                    ref CurrentTreasureFrameVAT frame, ref AnimationPlayState playState)
            {
                if (playState.Value == PlayState.Finished) return;

                ref var clips = ref animationLibrary.AnimationDataBlobRef.Value.Clips;
                // if ((int)TreasureAnimation.Open > clips.Length) return;

                switch (animation.Value)
                {
                    // case TreasureAnimation.Open:
                    //     if (PlayAnimation(ref frame.Value, clips[(int)TreasureAnimation.Open], in dt, resets: false, PlayDirection.Forward))
                    //     {
                    //         playState.Value = PlayState.Finished;
                    //     }
                    //     break;
                    // case TreasureAnimation.Close:
                    //     if (PlayAnimation(ref frame.Value, clips[(int)TreasureAnimation.Open], in dt, resets: false, PlayDirection.Reverse))
                    //     {
                    //         playState.Value = PlayState.Finished;
                    //     }
                    //     break;
                }
            }
        }

        /// <summary> This functions increase the frame's value. The Start Frame < End Frame. </summary>
        /// <param name="frame"> The _frame ref from the Material Property to control the current frame in the animation. </param>
        /// <param name="data"> `AnimationData` Regarding the animation parameters. </param>
        /// <param name="dt"> Time.DeltaTime </param>
        /// <param name="resets"> Does the animation frame value reset to Start? </param>
        /// <param name="playDirection"> Diretion to play the animation. </param>
        public static bool PlayAnimation(ref float frame, in AnimationData data, in float dt, bool resets, PlayDirection playDirection)
        {
            var signedRate = playDirection == PlayDirection.Forward ? data.FrameRate : -data.FrameRate;
            frame = math.clamp(frame + signedRate * dt, data.FrameRange.Start, data.FrameRange.End);
            bool outOfBounds = playDirection == PlayDirection.Forward ? frame >= data.FrameRange.End : frame <= data.FrameRange.Start;

            if (outOfBounds)
            {
                if (resets)
                {
                    frame = playDirection == PlayDirection.Forward ? data.FrameRange.Start : data.FrameRange.End;
                }
                if (!data.Loops) return true;
            }
            return false;
        }

        public enum PlayDirection
        {
            Forward,
            Reverse
        }
    }
}

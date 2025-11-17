using System;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    /* Notes
     * MO = MaterialOverride
     */

    public enum AnimationMode
    {
        Time = 1,
        Frame = 0
    }

    public enum Animations
    {
        Default,
        Idle,
        Walking,
        Mounting,
        Unmounting
    }

    public enum PlayState
    {
        NotPlaying,
        Paused,
        Playing,
        Finished
    }

    ///<summary> Store the first and last frame value of the animation.</summary>
    [Serializable]
    public struct FrameRange
    {
        public float Start;
        public float End;
    }

    [Serializable]
    public struct AnimationData
    {
        [Tooltip("The animation number for the animation.")]
        [SerializeField] public float Number;
        [Tooltip("The frame rate for the animation.")]
        [SerializeField] public float FrameRate;
        [Tooltip("The frame range for the animation.")]
        [SerializeField] public FrameRange FrameRange;
    }

    [MaterialProperty("_Animation_number")]
    public struct AnimationNumberMO : IComponentData { public float Value; }

    public struct AnimationPlayState : IComponentData { public PlayState Value; }

    #region 1
    [MaterialProperty("_1_frame")]
    public struct IdleFrameNumberMO : IComponentData { public float Value; }

    [MaterialProperty("_1_use_time")]
    public struct IdleUseTimeMO : IComponentData { public float Value; }

    [MaterialProperty("_1_speed")]
    public struct IdleSpeedMO : IComponentData { public float Value; }

    public struct IdleAnimationData : IComponentData { public AnimationData Data; }
    #endregion

    #region 2
    [MaterialProperty("_2_frame")]
    public struct WalkingFrameNumberMO : IComponentData { public float Value; }

    [MaterialProperty("_2_use_time")]
    public struct WalkingUseTimeMO : IComponentData { public float Value; }

    [MaterialProperty("_2_speed")]
    public struct WalkingSpeedMO : IComponentData { public float Value; }

    public struct WalkingAnimationData : IComponentData { public AnimationData Data; }
    #endregion

    #region 3
    [MaterialProperty("_3_frame")]
    public struct MountingFrameNumberMO : IComponentData { public float Value; }

    [MaterialProperty("_3_use_time")]
    public struct MountingUseTimeMO : IComponentData { public float Value; }

    [MaterialProperty("_3_speed")]
    public struct MountingSpeedMO : IComponentData { public float Value; }

    public struct MountingAnimationData : IComponentData { public AnimationData Data; }
    #endregion

    #region 4
    [MaterialProperty("_4_frame")]
    public struct UnmountingFrameNumberMO : IComponentData { public float Value; }

    [MaterialProperty("_4_use_time")]
    public struct UnmountingUseTimeMO : IComponentData { public float Value; }

    [MaterialProperty("_4_speed")]
    public struct UnmountingSpeedMO : IComponentData { public float Value; }

    public struct UnmountingAnimationData : IComponentData { public AnimationData Data; }
    #endregion

    public struct CurrentAnimation : IComponentData
    {
        public Animations Value;
    }
}

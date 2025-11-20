using System;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
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
        [Tooltip("The total number of frames for the animation.")]
        [SerializeField] public float TotalFrames;
        [Tooltip("The frame rate for the animation.")]
        [SerializeField] public float FrameRate;
        [Tooltip("The frame range for the animation.")]
        [SerializeField] public FrameRange FrameRange;
    }

    public struct AnimationPlayState : IComponentData { public PlayState Value; }

    [MaterialProperty("_current_frame")]
    public struct CurrentFrameVAT : IComponentData { public float Value; }

    [MaterialProperty("_use_time")]
    public struct UseTimeVAT : IComponentData { public float Value; }

    [MaterialProperty("_speed")]
    public struct SpeedVAT : IComponentData { public float Value; }

    public struct IdleAnimationData : IComponentData { public AnimationData Value; }
    public struct WalkingAnimationData : IComponentData { public AnimationData Value; }
    public struct MountingAnimationData : IComponentData { public AnimationData Value; }
    public struct UnmountingAnimationData : IComponentData { public AnimationData Value; }
    public struct CurrentAnimationData : IComponentData { public AnimationData Value; }
    public struct CurrentAnimationID : IComponentData { public Animations Value; }
}

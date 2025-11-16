using Unity.Entities;
using Unity.Rendering;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    [MaterialProperty("_Animation_number")]
    public struct MaterialOverrideAnimationNumber : IComponentData
    {
        public float Value;
    }

    public struct CoffeeAnimationPlayState : IComponentData
    {
        public PlayState Value;
    }

#region 1
    [MaterialProperty("_1_frame")]
    public struct MaterialOverride1FrameNumber : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_1_use_time")]
    public struct MaterialOverride1UseTime : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_1_speed")]
    public struct MaterialOverride1IdleSpeed : IComponentData
    {
        public float Value;
    }

    /// <summary> Store the frame rate for the idle animation</summary>
    public struct IdleFrameRangeComponent : IComponentData
    {
        public float Start;
        public float End;
    }

    /// <summary> Store the frame rate for the idle animation</summary>
    public struct IdleFrameRateComponent : IComponentData
    {
        public float Value;
    }

    public struct IdleAnimationPlayStateComponent : IComponentData
    {
        public PlayState Value;
    }
#endregion

#region 2
    [MaterialProperty("_2_frame")]
    public struct MaterialOverride2FrameNumber : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_2_use_time")]
    public struct MaterialOverride2UseTime : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_2_speed")]
    public struct MaterialOverride2IdleSpeed : IComponentData
    {
        public float Value;
    }

    /// <summary> Store the frame rate for the walking animation</summary>
    public struct WalkingFrameRangeComponent : IComponentData
    {
        public float Start;
        public float End;
    }

    /// <summary> Store the frame rate for the walking animation</summary>
    public struct WalkingFrameRateComponent : IComponentData
    {
        public float Value;
    }

    public struct WalkingAnimationPlayStateComponent : IComponentData
    {
        public PlayState Value;
    }
#endregion

#region 3
    [MaterialProperty("_3_frame")]
    public struct MaterialOverride3FrameNumber : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_3_use_time")]
    public struct MaterialOverride3UseTime : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_3_speed")]
    public struct MaterialOverride3IdleSpeed : IComponentData
    {
        public float Value;
    }

    /// <summary> Store the frame rate for the mounting animation</summary>
    public struct MountingFrameRangeComponent : IComponentData
    {
        public float Start;
        public float End;
    }

    /// <summary> Store the frame rate for the mounting animation</summary>
    public struct MountingFrameRateComponent : IComponentData
    {
        public float Value;
    }

    public struct MountingAnimationPlayStateComponent : IComponentData
    {
        public PlayState Value;
    }
#endregion

#region 4
    [MaterialProperty("_4_frame")]
    public struct MaterialOverride4FrameNumber : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_4_use_time")]
    public struct MaterialOverride4UseTime : IComponentData
    {
        public float Value;
    }

    [MaterialProperty("_4_speed")]
    public struct MaterialOverride4IdleSpeed : IComponentData
    {
        public float Value;
    }

    /// <summary> Store the frame rate for the unmounting animation</summary>
    public struct UnmountingFrameRangeComponent : IComponentData
    {
        public float Start;
        public float End;
    }

    /// <summary> Store the frame rate for the unmounting animation</summary>
    public struct UnmountingFrameRateComponent : IComponentData
    {
        public float Value;
    }

    public struct UnmountingAnimationPlayStateComponent : IComponentData
    {
        public PlayState Value;
    }
#endregion

    // TODO: rename CoffeeAnimation
    public struct CoffeeAnimationComponent : IComponentData
    {
        public CoffeeAnimation Value;
    }

#region Animation Number Components
    /// <summary> Store the idle animation number for the character animation</summary>
    public struct IdleAnimationNumber : IComponentData
    {
        public float Value;
    }

    /// <summary> Stores the walking animation number for the character animation</summary>
    public struct WalkingAnimationNumber : IComponentData
    {
        public float Value;
    }

    public struct MountingAnimationNumber : IComponentData
    {
        public float Value;
    }

    public struct UnmountingAnimationNumber : IComponentData
    {
        public float Value;
    }
#endregion

#region enums
    public enum CoffeeAnimation
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
#endregion
}

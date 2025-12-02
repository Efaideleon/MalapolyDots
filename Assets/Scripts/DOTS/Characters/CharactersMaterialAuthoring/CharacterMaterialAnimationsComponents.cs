using System;
using Unity.Entities;
using Unity.Rendering;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public enum AnimationMode
    {
        Time = 1,
        Frame = 0
    }

    public enum CharacterAnimations
    {
        Default,
        Idle,
        Walking,
        Mounting,
        Unmounting
    }

    public enum TreasureAnimations
    {
        Default,
        Open,
        Close
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

    public struct AnimationData
    {
        public float FrameRate;
        public FrameRange FrameRange;
        public bool Loops;
    }

    /// <summary> 
    /// Holds a BlobArray for all the AnimationData.
    /// The Clips are access by the Animations enums used during BlobAsset creation.
    /// </summary>
    public struct AnimationDataBlob
    {
        public BlobArray<AnimationData> Clips;
    }

    /// <summary> Holds a reference to the AnimationDataBlobAsset.</summary>
    public struct AnimationDataLibrary : IComponentData
    {
        public BlobAssetReference<AnimationDataBlob> AnimationDataBlobRef;
    }

    public struct AnimationPlayState : IComponentData { public PlayState Value; }

    [MaterialProperty("_current_frame")]
    public struct CurrentFrameVAT : IComponentData { public float Value; }

    [MaterialProperty("_frame")]
    public struct CurrentTreasureFrameVAT : IComponentData { public float Value; }

    [MaterialProperty("_use_time")]
    public struct UseTimeVAT : IComponentData { public float Value; }

    [MaterialProperty("_speed")]
    public struct SpeedVAT : IComponentData { public float Value; }

    public struct IdleAnimationData : IComponentData { public AnimationData Value; }
    public struct WalkingAnimationData : IComponentData { public AnimationData Value; }
    public struct MountingAnimationData : IComponentData { public AnimationData Value; }
    public struct UnmountingAnimationData : IComponentData { public AnimationData Value; }
    public struct CurrentCharacterAnimation : IComponentData { public CharacterAnimations Value; }
    public struct CurrentTreasureAnimation : IComponentData { public TreasureAnimations Value; }
}

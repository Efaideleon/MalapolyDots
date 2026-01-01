using System;
using Assets.Scripts.DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.Characters.CharactersMaterialAuthoring.ScriptableObjects;
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

    public enum CharacterAnimation
    {
        Default,
        Idle,
        Walking,
        Mounting,
        Unmounting
    }

    public enum TreasureAnimation
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


    [Serializable]
    public class CharacterAnimations
    {
        [Header("Idle")]
        public AnimationPhaseGroup Idle;
        [Header("Walking")]
        public AnimationPhaseGroup Walking;
    }

    [Serializable]
    public class AnimationPhaseGroup
    {
        public AnimationDataSO Start;
        public AnimationDataSO Middle;
        public AnimationDataSO End;

        public AnimationPhaseGroupData ToAnimationPhaseGroupData()
        {
            return new AnimationPhaseGroupData
            {
                Start = Start == null ? new() : Start.ToAnimationData(),
                Middle = Middle == null ? new() : Middle.ToAnimationData(),
                End = End == null ? new() : End.ToAnimationData()
            };
        }
    }

    public struct AnimationPhaseGroupData
    {
        public AnimationData Start;
        public AnimationData Middle;
        public AnimationData End;
    }

    public struct AnimationData
    {
        public float FrameRate;
        public FrameRange FrameRange;
        public bool Loops;
        public bool HasClip;
    }

    /// <summary> 
    /// Holds a BlobArray for all the AnimationData.
    /// The Clips are access by the Animations enums used during BlobAsset creation.
    /// </summary>
    public struct AnimationDataBlob
    {
        public BlobArray<AnimationPhaseGroupData> Clips;
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
    public struct CurrentCharacterAnimation : IComponentData { public CharacterAnimation Value; }
    public struct CurrentTreasureAnimation : IComponentData { public TreasureAnimation Value; }
}

using System;
using DOTS.Characters.CharactersMaterialAuthoring;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.Characters.CharactersMaterialAuthoring
{
    public class CharactersAnimationAuthoring : MonoBehaviour
    {
        [Header("Animations Clips")]
        public CharacterAnimations animations;

        [Header("Animations Settings")]
        [SerializeField] float UseTime;
        [SerializeField] float Speed;

        class CharactersAnimationBaker : Baker<CharactersAnimationAuthoring>
        {
            public override void Bake(CharactersAnimationAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                using var builder = new BlobBuilder(Allocator.Temp);

                ref var root = ref builder.ConstructRoot<AnimationDataBlob>();

                BlobBuilderArray<AnimationPhaseGroupData> clipsArray = builder.Allocate(ref root.Clips, Enum.GetValues(typeof(CharacterAnimationEnum)).Length);

                clipsArray[(int)CharacterAnimationEnum.Idle] = authoring.animations.Idle.ToAnimationPhaseGroupData();
                clipsArray[(int)CharacterAnimationEnum.Walking] = authoring.animations.Walking.ToAnimationPhaseGroupData();

                var blobRef = builder.CreateBlobAssetReference<AnimationDataBlob>(Allocator.Persistent);
                AddBlobAsset(ref blobRef, out _);
                var animationDataLibrary = new AnimationDataLibrary { AnimationDataBlobRef = blobRef };
                AddComponent(entity, animationDataLibrary);
                AddComponent(entity, new CurrentFrameVAT { Value = 0 });
                AddComponent(entity, new UseTimeVAT { Value = authoring.UseTime });
                AddComponent(entity, new SpeedVAT { Value = authoring.Speed });
                AddComponent(entity, new AnimationPlayState { Value = PlayState.NotPlaying });
                AddComponent(entity, new CurrentCharacterAnimation { Value = CharacterAnimation.Default });
                AddComponent(entity, new AnimationPhaseComponent { Value = AnimationPhase.None });
                AddComponent(entity, new CharacterAnimationState { Value = CharacterAnimationEnum.None });
                AddComponent(entity, new ActiveAnimation { Value = default });
                AddComponent(entity, new PreviousAnimationPlayState { Value = PlayState.NotPlaying });
                AddComponent(entity, new AnimationPhaseRequestComponent { PhaseRequested = AnimationPhase.None, Signals = AnimationSignals.None });

                AddComponent(entity, new DesiredAnimation { Value = CharacterAnimationEnum.None });
                AddComponent(entity, new AnimationStateComponent
                {
                    CurrentAnimation = CharacterAnimationEnum.None,
                    PendingAnimation = CharacterAnimationEnum.None,
                    Frame = 0,
                    Phase = AnimationPhase.Start
                });
            }
        }
    }

    public struct AnimationStateComponent : IComponentData
    {
        public CharacterAnimationEnum CurrentAnimation;
        public CharacterAnimationEnum PendingAnimation;
        public float Frame;
        public AnimationPhase Phase;
    }

    public struct DesiredAnimation : IComponentData
    {
        public CharacterAnimationEnum Value;
    }

    public struct CharacterAnimationState : IComponentData
    {
        public CharacterAnimationEnum Value;
    }

    public struct AnimationPhaseComponent : IComponentData
    {
        public AnimationPhase Value;
    }

    public struct AnimationPhaseRequestComponent : IComponentData
    {
        public AnimationPhase PhaseRequested;
        public AnimationSignals Signals;
    }

    public struct ActiveAnimation : IComponentData
    {
        public AnimationData Value;
    }

    public struct PreviousAnimationPlayState : IComponentData
    {
        public PlayState Value;
    }

    public enum AnimationSignals : byte
    {
        None = 0,
        AdvancePhase = 1 << 0,
        PhaseChanged = 1 << 1,
        Blend = 1 << 2
    }

    // Make into a byte type
    public enum AnimationPhase
    {
        None,
        Start,
        Middle,
        End
    }

    public enum CharacterAnimationEnum
    {
        None,
        Idle,
        Walking
    }
}

using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class CoffeeMaterialAuthoring : MonoBehaviour
    {
        [Header("Idle Animation")]
        [Tooltip("The animation number for the idle animation.")]
        [SerializeField] float IDLE;

        [Tooltip("The frame rate for the idle animation.")]
        [SerializeField] float IDLE_FrameRate;

        [Tooltip("The start frame for the idle animation.")]
        [SerializeField] float IDLE_startFrame;

        [Tooltip("The end frame for the idle animation.")]
        [SerializeField] float IDLE_endFrame;

        [Header("Walking Animation")]
        [Tooltip("The animation number for the walking animation.")]
        [SerializeField] float WALKING;

        [Tooltip("The frame rate for the walking animation.")]
        [SerializeField] float Walking_FrameRate;

        [Tooltip("The start frame for the walking animation.")]
        [SerializeField] float Walking_startFrame;

        [Tooltip("The end frame for the walking animation.")]
        [SerializeField] float Walking_endFrame;

        [Header("Mounting Animation")]
        [Tooltip("The animation number for the mounting animation.")]
        [SerializeField] float MOUNTING;

        [Tooltip("The frame rate for the mounting animation.")]
        [SerializeField] float Mounting_FrameRate;

        [Tooltip("The start frame for the mounting animation.")]
        [SerializeField] float Mounting_startFrame;

        [Tooltip("The end frame for the mounting animation.")]
        [SerializeField] float Mounting_endFrame;

        [Header("Unmounting Animation")]
        [Tooltip("The animation number for the umounting animation.")]
        [SerializeField] float UNMOUNTING;

        [Tooltip("The frame rate for the unmounting animation.")]
        [SerializeField] float Unmounting_FrameRate;

        [Tooltip("The start frame for the unmounting animation.")]
        [SerializeField] float Unmounting_startFrame;

        [Tooltip("The end frame for the unmounting animation.")]
        [SerializeField] float Unmounting_endFrame;

        class CoffeeMaterialBaker : Baker<CoffeeMaterialAuthoring>
        {
            public override void Bake(CoffeeMaterialAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new MaterialOverrideAnimationNumber { Value = 0 });

                // Idle
                AddComponent(entity, new MaterialOverride1FrameNumber { Value = 0 });
                AddComponent(entity, new MaterialOverride1UseTime { Value = (float)AnimationMode.Frame });
                AddComponent(entity, new MaterialOverride1IdleSpeed { Value = 1 });
                AddComponent(entity, new IdleComponent { Value = authoring.IDLE });
                AddComponent(entity, new IdleFrameRangeComponent { Start = authoring.IDLE_startFrame, End = authoring.IDLE_endFrame });
                AddComponent(entity, new IdleFrameRateComponent { Value = authoring.IDLE_FrameRate });

                // Walking
                var walkingAnimationEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                AddComponent(walkingAnimationEntity, new MaterialOverride2FrameNumber { Value = 0 });
                AddComponent(walkingAnimationEntity, new MaterialOverride2UseTime { Value = (float)AnimationMode.Frame });
                AddComponent(walkingAnimationEntity, new MaterialOverride2IdleSpeed { Value = 1 });
                AddComponent(walkingAnimationEntity, new WalkingComponent { Value = authoring.WALKING });
                AddComponent(walkingAnimationEntity, new WalkingFrameRangeComponent { Start = authoring.Walking_startFrame, End = authoring.Walking_endFrame });
                AddComponent(walkingAnimationEntity, new WalkingFrameRateComponent { Value = authoring.Walking_FrameRate });
                AddComponent(walkingAnimationEntity, new AnimationStateComponent { Value = CharactersAnimationState.NotPlaying });

                // Mounting
                var mountingAnimationEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                AddComponent(mountingAnimationEntity, new MaterialOverride3FrameNumber { Value = 0 });
                AddComponent(mountingAnimationEntity, new MaterialOverride3UseTime { Value = (float)AnimationMode.Frame });
                AddComponent(mountingAnimationEntity, new MaterialOverride3IdleSpeed { Value = 1 });
                AddComponent(mountingAnimationEntity, new MountingComponent { Value = authoring.MOUNTING });
                AddComponent(mountingAnimationEntity, new MountingFrameRangeComponent { Start = authoring.Mounting_startFrame, End = authoring.Mounting_endFrame });
                AddComponent(mountingAnimationEntity, new MountingFrameRateComponent { Value = authoring.Mounting_FrameRate });
                AddComponent(mountingAnimationEntity, new AnimationStateComponent { Value = CharactersAnimationState.NotPlaying });

                // Unmounting
                AddComponent(entity, new MaterialOverride4FrameNumber { Value = 0 });
                AddComponent(entity, new MaterialOverride4UseTime { Value = (float)AnimationMode.Frame });
                AddComponent(entity, new MaterialOverride4IdleSpeed { Value = 1 });
                AddComponent(entity, new UnmountingComponent { Value = authoring.UNMOUNTING });
                AddComponent(entity, new UnmountingFrameRangeComponent { Start = authoring.Unmounting_startFrame, End = authoring.Unmounting_endFrame });
                AddComponent(entity, new UnmountingFrameRateComponent { Value = authoring.Unmounting_FrameRate });

                AddComponent(entity, new CoffeeAnimationStateComponent { Value = CoffeeAnimationState.Idle });
                AddComponent(entity, new CoffeeMaterialTag { });
            }
        }
    }

    public struct CoffeeMaterialTag : IComponentData { }

    public enum AnimationMode
    {
        Time = 1,
        Frame = 0
    }
}

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
                AddComponent(entity, new CoffeeAnimationPlayState { Value = PlayState.NotPlaying });

                AddComponent(entity, new MaterialOverride1FrameNumber { Value = 0 });
                AddComponent(entity, new MaterialOverride1UseTime { Value = (float)AnimationMode.Frame });
                AddComponent(entity, new MaterialOverride1IdleSpeed { Value = 1 });

                AddComponent(entity, new MaterialOverride2FrameNumber { Value = 0 });
                AddComponent(entity, new MaterialOverride2UseTime { Value = (float)AnimationMode.Frame });
                AddComponent(entity, new MaterialOverride2IdleSpeed { Value = 1 });

                AddComponent(entity, new MaterialOverride3FrameNumber { Value = 0 });
                AddComponent(entity, new MaterialOverride3UseTime { Value = (float)AnimationMode.Frame });
                AddComponent(entity, new MaterialOverride3IdleSpeed { Value = 1 });

                AddComponent(entity, new MaterialOverride4FrameNumber { Value = 0 });
                AddComponent(entity, new MaterialOverride4UseTime { Value = (float)AnimationMode.Frame });
                AddComponent(entity, new MaterialOverride4IdleSpeed { Value = 1 });

                // Idle
                AddComponent(entity, new IdleAnimationNumber { Value = authoring.IDLE });
                AddComponent(entity, new IdleFrameRangeComponent { Start = authoring.IDLE_startFrame, End = authoring.IDLE_endFrame });
                AddComponent(entity, new IdleFrameRateComponent { Value = authoring.IDLE_FrameRate });

                // Walking
                AddComponent(entity, new WalkingAnimationNumber { Value = authoring.WALKING });
                AddComponent(entity, new WalkingFrameRangeComponent { Start = authoring.Walking_startFrame, End = authoring.Walking_endFrame });
                AddComponent(entity, new WalkingFrameRateComponent { Value = authoring.Walking_FrameRate });

                // Mounting
                AddComponent(entity, new MountingAnimationNumber { Value = authoring.MOUNTING });
                AddComponent(entity, new MountingFrameRangeComponent { Start = authoring.Mounting_startFrame, End = authoring.Mounting_endFrame });
                AddComponent(entity, new MountingFrameRateComponent { Value = authoring.Mounting_FrameRate });

                // Unmounting
                AddComponent(entity, new UnmountingAnimationNumber { Value = authoring.UNMOUNTING });
                AddComponent(entity, new UnmountingFrameRangeComponent { Start = authoring.Unmounting_startFrame, End = authoring.Unmounting_endFrame });
                AddComponent(entity, new UnmountingFrameRateComponent { Value = authoring.Unmounting_FrameRate });

                AddComponent(entity, new CoffeeAnimationComponent { Value = CoffeeAnimation.Idle });
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

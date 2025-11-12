using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class CoffeeMaterialAuthoring : MonoBehaviour
    {
        [Header("Idle Animation")]
        [Tooltip("The animation number for the idle animation.")]
        [SerializeField] float IDLE;

        [Tooltip("The frame rate for the  idle animation.")]
        [SerializeField] float IDLE_FrameRate;

        [Tooltip("The start frame for the  idle animation.")]
        [SerializeField] float IDLE_startFrame;

        [Tooltip("The end frame for the  idle animation.")]
        [SerializeField] float IDLE_endFrame;

        [Header("Walking Animation")]
        [Tooltip("The animation number for the walking animation.")]
        [SerializeField] float WALKING;

        class CoffeeMaterialBaker : Baker<CoffeeMaterialAuthoring>
        {
            public override void Bake(CoffeeMaterialAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new MaterialOverrideAnimationNumber { Value = 0 });
                AddComponent(entity, new MaterialOverrideFrameNumber { Value = 0 });
                AddComponent(entity, new MaterialOverrideUseTime { Value = (float)AnimationMode.Frame });
                AddComponent(entity, new MaterialOverrideIdleSpeed { Value = 1 });
                AddComponent(entity, new IdleComponent { Value = authoring.IDLE });
                AddComponent(entity, new WalkingComponent { Value = authoring.WALKING });
                AddComponent(entity, new IdleFrameRateComponent { Value = authoring.IDLE_FrameRate });
                AddComponent(entity, new CoffeeMaterialTag { });
                AddComponent(entity, new IdleFrameRangeComponent { Start = authoring.IDLE_startFrame, End = authoring.IDLE_endFrame });
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

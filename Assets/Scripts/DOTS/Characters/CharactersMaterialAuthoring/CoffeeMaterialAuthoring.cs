using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class CoffeeMaterialAuthoring : MonoBehaviour
    {
        [Header("Animation Number")]
        [Tooltip("The animation number for the idle animation.")]
        [SerializeField] float IDLE;

        [Tooltip("The animation number for the walking animation.")]
        [SerializeField] float WALKING;

        class CoffeeMaterialBaker : Baker<CoffeeMaterialAuthoring>
        {
            public override void Bake(CoffeeMaterialAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new MaterialOverrideAnimationNumber { Value = 0 });
                AddComponent(entity, new MaterialOverrideFrameNumber { Value = 0 });
                AddComponent(entity, new MaterialOverrideUseTime { Value = false });
                AddComponent(entity, new IdleComponent { Value = authoring.IDLE });
                AddComponent(entity, new WalkingComponent { Value = authoring.WALKING });
                AddComponent(entity, new CoffeeMaterialTag { });
            }
        }
    }

    public struct CoffeeMaterialTag : IComponentData { }
}

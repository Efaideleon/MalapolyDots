using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class BirdMaterialAuthoring : MonoBehaviour
    {
        [Header("Animation Number")]
        [Tooltip("The animation number for the idle animation.")]
        [SerializeField] float IDLE;

        [Tooltip("The animation number for the walking animation.")]
        [SerializeField] float WALKING;

        class BirdMaterialBaker : Baker<BirdMaterialAuthoring>
        {
            public override void Bake(BirdMaterialAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new MaterialOverrideAnimationNumber { Value = 0 });
                AddComponent(entity, new IdleComponent { Value = authoring.IDLE });
                AddComponent(entity, new WalkingComponent { Value = authoring.WALKING });
                AddComponent(entity, new BirdMaterialTag { });
            }
        }
    }

    public struct BirdMaterialTag : IComponentData { }
}

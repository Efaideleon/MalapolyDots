using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class TuctucMaterialAuthoring : MonoBehaviour
    {
        [Header("Animation Number")]
        [Tooltip("The animation number for the idle animation.")]
        [SerializeField] float IDLE;

        [Tooltip("The animation number for the walking animation.")]
        [SerializeField] float WALKING;

        class TuctucMaterialBaker : Baker<TuctucMaterialAuthoring>
        {
            public override void Bake(TuctucMaterialAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new MaterialOverrideAnimationNumber { Value = 0 });
                AddComponent(entity, new IdleComponent { Value = authoring.IDLE });
                AddComponent(entity, new WalkingComponent { Value = authoring.WALKING });
                AddComponent(entity, new TuctucMaterialTag { });
            }
        }
    }

    public struct TuctucMaterialTag : IComponentData { }
}

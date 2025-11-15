using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class AvocadoMaterialAuthoring : MonoBehaviour
    {
        [Header("Animation Number")]
        [Tooltip("The animation number for the idle animation.")]
        [SerializeField] float IDLE;

        [Tooltip("The animation number for the walking animation.")]
        [SerializeField] float WALKING;

        class AvocadoMaterialBaker : Baker<AvocadoMaterialAuthoring>
        {
            public override void Bake(AvocadoMaterialAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new MaterialOverrideAnimationNumber { Value = 0 });
                AddComponent(entity, new IdleAnimationNumber { Value = authoring.IDLE });
                AddComponent(entity, new WalkingAnimationNumber { Value = authoring.WALKING });
                AddComponent(entity, new AvocadoMaterialTag { });
            }
        }
    }

    public struct AvocadoMaterialTag : IComponentData { }
}

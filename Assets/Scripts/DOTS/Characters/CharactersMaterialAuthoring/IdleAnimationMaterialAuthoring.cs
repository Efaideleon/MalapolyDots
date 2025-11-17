using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class IdleAnimationMaterialAuthoring : MonoBehaviour
    {
        [Header("Idle Animation")]
        [SerializeField] AnimationData Data;

        class IdleMaterialBaker : Baker<IdleAnimationMaterialAuthoring>
        {
            public override void Bake(IdleAnimationMaterialAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new IdleFrameNumberMO { Value = 0 });
                AddComponent(entity, new IdleUseTimeMO { Value = (float)AnimationMode.Frame });
                AddComponent(entity, new IdleSpeedMO { Value = 1 });
                AddComponent(entity, new IdleAnimationData { Data = authoring.Data });
            }
        }
    }
}

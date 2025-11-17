using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class MountingAnimationMaterialAuthoring : MonoBehaviour
    {
        [Header("Mounting Animation")]
        [SerializeField] AnimationData Data;

        class MountingMaterialBaker : Baker<MountingAnimationMaterialAuthoring>
        {
            public override void Bake(MountingAnimationMaterialAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new MountingFrameNumberMO { Value = 0 });
                AddComponent(entity, new MountingUseTimeMO { Value = (float)AnimationMode.Frame });
                AddComponent(entity, new MountingSpeedMO { Value = 1 });
                AddComponent(entity, new MountingAnimationData { Data = authoring.Data });
            }
        }
    }
}

using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class UnmountingAnimationMaterialAuthoring : MonoBehaviour
    {
        [Header("Unmounting Animation")]
        [SerializeField] AnimationData Data;
        class UnmountingMaterialBaker : Baker<UnmountingAnimationMaterialAuthoring>
        {
            public override void Bake(UnmountingAnimationMaterialAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new UnmountingFrameNumberMO { Value = 0 });
                AddComponent(entity, new UnmountingUseTimeMO { Value = (float)AnimationMode.Frame });
                AddComponent(entity, new UnmountingSpeedMO { Value = 1 });
                AddComponent(entity, new UnmountingAnimationData { Data = authoring.Data });
                AddComponent(entity, new UnmountingAnimationTag { });
            }
        }
    }
}

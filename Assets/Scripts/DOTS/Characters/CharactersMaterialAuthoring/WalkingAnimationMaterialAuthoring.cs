using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class WalkingAnimationMaterialAuthoring : MonoBehaviour
    {
        [Header("Walking Animation")]
        [SerializeField] AnimationData Data;

        class WalkingMaterialBaker : Baker<WalkingAnimationMaterialAuthoring>
        {
            public override void Bake(WalkingAnimationMaterialAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new WalkingFrameNumberMO { Value = 0 });
                AddComponent(entity, new WalkingUseTimeMO { Value = (float)AnimationMode.Frame });
                AddComponent(entity, new WalkingSpeedMO { Value = 1 });
                AddComponent(entity, new WalkingAnimationData { Data = authoring.Data });
                AddComponent(entity, new WalkingAnimationTag { });
            }
        }
    }
}

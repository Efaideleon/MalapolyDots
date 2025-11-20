using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class BirdAnimationAuthoring : MonoBehaviour
    {
        [Header("Idle Animation")]
        [SerializeField] AnimationData IdleData;
        [Header("Walking Animation")]
        [SerializeField] AnimationData WalkingData;

        [Header("Animations Settings")]
        [SerializeField] float UseTime;
        [SerializeField] float Speed;

        class BirdAnimationBaker : Baker<BirdAnimationAuthoring>
        {
            public override void Bake(BirdAnimationAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new IdleAnimationData { Value = authoring.IdleData });
                AddComponent(entity, new WalkingAnimationData { Value = authoring.WalkingData });
                AddComponent(entity, new CurrentAnimationData { Value = authoring.IdleData });
                AddComponent(entity, new CurrentFrameVAT { Value = 0 });
                AddComponent(entity, new UseTimeVAT { Value = authoring.UseTime });
                AddComponent(entity, new SpeedVAT { Value = authoring.Speed });
                AddComponent(entity, new AnimationPlayState { Value = PlayState.NotPlaying });
                AddComponent(entity, new BirdMaterialTag { });
            }
        }
    }

    public struct BirdMaterialTag : IComponentData { }
}

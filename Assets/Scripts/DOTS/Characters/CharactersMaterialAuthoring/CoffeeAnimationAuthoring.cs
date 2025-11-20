using Unity.Entities;
using UnityEngine;

namespace DOTS.Characters.CharactersMaterialAuthoring
{
    public class CoffeeAnimationAuthoring : MonoBehaviour
    {
        [Header("Idle Animation")]
        [SerializeField] AnimationData IdleData;
        [Header("Walking Animation")]
        [SerializeField] AnimationData WalkingData;
        [Header("Mounting Animation")]
        [SerializeField] AnimationData MountingData;
        [Header("Unmounting Animation")]
        [SerializeField] AnimationData UnmountingData;

        [Header("Animations Settings")]
        [SerializeField] float UseTime;
        [SerializeField] float Speed;

        class CoffeeAnimationBaker : Baker<CoffeeAnimationAuthoring>
        {
            public override void Bake(CoffeeAnimationAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new IdleAnimationData { Value = authoring.IdleData });
                AddComponent(entity, new WalkingAnimationData { Value = authoring.WalkingData });
                AddComponent(entity, new CurrentAnimationData { Value = authoring.IdleData });
                AddComponent(entity, new CurrentFrameVAT { Value = 0 });
                AddComponent(entity, new UseTimeVAT { Value = authoring.UseTime });
                AddComponent(entity, new SpeedVAT { Value = authoring.Speed });
                AddComponent(entity, new AnimationPlayState { Value = PlayState.NotPlaying });
                AddComponent(entity, new CurrentAnimationID { Value = Animations.Idle });
                AddComponent(entity, new CoffeeMaterialTag { });
            }
        }
    }

    public struct CoffeeMaterialTag : IComponentData { }
}

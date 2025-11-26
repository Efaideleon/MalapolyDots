using System;
using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.Characters.CharactersMaterialAuthoring.ScriptableObjects;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.DOTS.Characters.CharactersMaterialAuthoring
{
    public class CharactersAnimationAuthoring : MonoBehaviour
    {
        [Header("Animations Clips")]
        public AnimationDataSO[] AnimationClips;

        [Header("Animations Settings")]
        [SerializeField] float UseTime;
        [SerializeField] float Speed;

        class CharactersAnimationBaker : Baker<CharactersAnimationAuthoring>
        {
            public override void Bake(CharactersAnimationAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);

                using var builder = new BlobBuilder(Allocator.Temp);
                
                ref var root = ref builder.ConstructRoot<AnimationDataBlob>();

                var animationsClips = builder.Allocate(ref root.Clips, Enum.GetValues(typeof(Animations)).Length);

                for (int i = 0; i < authoring.AnimationClips.Length; i++)
                {
                    var clip = authoring.AnimationClips[i];
                    animationsClips[(int)clip.AnimationEnum] = clip.ToAnimationData();
                }

                var blobRef = builder.CreateBlobAssetReference<AnimationDataBlob>(Allocator.Persistent);

                var animationDataLibrary = new AnimationDataLibrary { AnimationDataBlobRef = blobRef };
                AddComponent(entity, animationDataLibrary);
                AddComponent(entity, new CurrentAnimationData { Value = animationDataLibrary.AnimationDataBlobRef.Value.Clips[(int)Animations.Idle] });
                AddComponent(entity, new CurrentFrameVAT { Value = 0 });
                AddComponent(entity, new UseTimeVAT { Value = authoring.UseTime });
                AddComponent(entity, new SpeedVAT { Value = authoring.Speed });
                AddComponent(entity, new AnimationPlayState { Value = PlayState.NotPlaying });
                AddComponent(entity, new CurrentAnimationID { Value = Animations.Idle });
            }
        }
    }
}

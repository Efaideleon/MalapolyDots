using System;
using DOTS.Characters.CharactersMaterialAuthoring;
using DOTS.Characters.CharactersMaterialAuthoring.ScriptableObjects;
using DOTS.DataComponents;
using DOTS.GameData.PlacesData;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DOTS.GameSpaces
{
    public class TreasureSpaceAuthoring : MonoBehaviour
    {
        public TreasureSpaceData Data;
        [Header("Animations Clips")]
        public AnimationTreasureDataSO[] AnimationClips;

        class TreasureSpaceAuthoringBaker : Baker<TreasureSpaceAuthoring>
        {
            public override void Bake(TreasureSpaceAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                using var builder = new BlobBuilder(Allocator.Temp);

                ref var root = ref builder.ConstructRoot<AnimationDataBlob>();

                var animationsClips = builder.Allocate(ref root.Clips, Enum.GetValues(typeof(TreasureAnimations)).Length);

                for (int i = 0; i < authoring.AnimationClips.Length; i++)
                {
                    var clip = authoring.AnimationClips[i];
                    animationsClips[(int)clip.AnimationEnum] = clip.ToAnimationData();
                }

                var blobRef = builder.CreateBlobAssetReference<AnimationDataBlob>(Allocator.Persistent);

                var animationDataLibrary = new AnimationDataLibrary { AnimationDataBlobRef = blobRef };
                AddComponent(entity, animationDataLibrary);
                AddComponent(entity, new NameComponent { Value = authoring.Data.Name });
                AddComponent(entity, new SpaceIDComponent { Value = authoring.Data.id });
                AddComponent(entity, new BoardIndexComponent { Value = default });
                AddComponent(entity, new TreasureSpaceTag { });
                AddComponent(entity, new SpaceTypeComponent { Value = authoring.Data.SpaceType });
                AddComponent(entity, new AnimationPlayState { Value = PlayState.NotPlaying });
                AddComponent(entity, new CurrentTreasureAnimation { Value = TreasureAnimations.Close });
                AddComponent(entity, new CurrentTreasureFrameVAT { });

                var cards = AddBuffer<TreasureCardsBuffer>(entity);
                foreach (var card in authoring.Data.Cards)
                {
                    cards.Add(new TreasureCardsBuffer { id = card.id, msg = card.msg });
                }
            }
        }
    }

    public struct TreasureSpaceTag : IComponentData
    { }

    public struct TreasureCardsBuffer : IBufferElementData
    {
        public int id;
        public FixedString64Bytes msg;
    }
}

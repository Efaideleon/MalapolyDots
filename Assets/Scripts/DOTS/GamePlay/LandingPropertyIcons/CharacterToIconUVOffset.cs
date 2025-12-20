using DOTS.Characters;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS.GamePlay.LandingPropertyIcons
{
    public class CharacterToIconUVOffset : MonoBehaviour
    {
        public class CharacterToIconBaker : Baker<CharacterToIconUVOffset>
        {
            public override void Bake(CharacterToIconUVOffset authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<CharacterToIconBlobAsset>();
                var arrayBuilder = builder.Allocate(ref root.array, 7); 

                arrayBuilder[(int)CharactersEnum.Default] = new float2(0.33f, 0);
                arrayBuilder[(int)CharactersEnum.Avocado] = new float2(0, 0.66f);
                arrayBuilder[(int)CharactersEnum.Coffee] = new float2(0.33f, 0.66f);
                arrayBuilder[(int)CharactersEnum.Bird] = new float2(0.66f, 0.66f);
                arrayBuilder[(int)CharactersEnum.Lira] = new float2(0, 0.33f);
                arrayBuilder[(int)CharactersEnum.Tuctuc] = new float2(0.33f, 0.33f);
                arrayBuilder[(int)CharactersEnum.Coin] = new float2(0.66f, 0.33f);

                var blobAssetRef = builder.CreateBlobAssetReference<CharacterToIconBlobAsset>(Allocator.Persistent);
                AddBlobAsset(ref blobAssetRef, out var hash);
                AddComponent(entity, new CharacterNametoIconUVMapBlobReference { map = blobAssetRef });
                builder.Dispose();
            }
        }
    }

    public struct CharacterToIconBlobAsset
    {
        public BlobArray<float2> array; 
    }

    public struct CharacterNametoIconUVMapBlobReference : IComponentData
    {
        public BlobAssetReference<CharacterToIconBlobAsset> map;
    }
}

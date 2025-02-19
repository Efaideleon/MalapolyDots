using Unity.Entities;

namespace DOTS
{
    public struct GameDataBlobComponent : IComponentData
    {
        public BlobAssetReference<GameDataBlob> gameDataBlob;
    }
}

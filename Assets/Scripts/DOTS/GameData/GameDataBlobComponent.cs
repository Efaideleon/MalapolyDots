using Unity.Entities;

namespace DOTS.GameData
{
    public struct GameDataBlobComponent : IComponentData
    {
        public BlobAssetReference<GameDataBlob> gameDataBlob;
    }
}

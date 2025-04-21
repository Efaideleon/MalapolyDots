using Unity.Entities;

namespace DOTS.Utilities.TreasuresBlob
{
    public struct TreasuresDataBlobReference : IComponentData
    {
        public BlobAssetReference<TreasureDataBlob> treasuresBlobReference;
    }
}

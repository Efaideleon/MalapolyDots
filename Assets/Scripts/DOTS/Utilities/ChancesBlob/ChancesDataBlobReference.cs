using Unity.Entities;

namespace DOTS.Utilities.ChancesBlob
{
    public struct ChancesDataBlobReference : IComponentData
    {
        public BlobAssetReference<ChancesDataBlob> chancesBlobReference;
    }
}

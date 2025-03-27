using Unity.Entities;

public struct ChancesDataBlobReference : IComponentData
{
    public BlobAssetReference<ChancesDataBlob> chancesBlobReference;
}

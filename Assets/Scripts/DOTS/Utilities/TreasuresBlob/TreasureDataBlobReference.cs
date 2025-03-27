using Unity.Entities;

public struct TreasuresDataBlobReference : IComponentData
{
    public BlobAssetReference<TreasureDataBlob> treasuresBlobReference;
}

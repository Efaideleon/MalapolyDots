using Unity.Entities;

public struct GoDataBlobReference : IComponentData
{
    public BlobAssetReference<GoDataBlob> goBlobReference;
}

using Unity.Entities;

public struct GoToJailDataBlobReference : IComponentData
{
    public BlobAssetReference<GoToJailDataBlob> goToJailBlobReference;
}

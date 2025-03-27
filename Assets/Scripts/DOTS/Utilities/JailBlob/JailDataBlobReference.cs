using Unity.Entities;

public struct JailDataBlobReference : IComponentData
{
    public BlobAssetReference<JailDataBlob> jailBlobReference;
}

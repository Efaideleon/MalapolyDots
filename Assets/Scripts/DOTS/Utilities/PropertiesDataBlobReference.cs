using Unity.Entities;

public struct PropertiesDataBlobReference : IComponentData
{
    public BlobAssetReference<PropertiesDataBlob> propertiesBlobReference;
}

using Unity.Entities;

public struct TaxesDataBlobReference : IComponentData
{
    public BlobAssetReference<TaxesDataBlob> taxesBlobReference;
}

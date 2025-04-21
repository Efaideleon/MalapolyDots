using Unity.Entities;

namespace DOTS.Utilities.TaxesBlob
{
    public struct TaxesDataBlobReference : IComponentData
    {
        public BlobAssetReference<TaxesDataBlob> taxesBlobReference;
    }
}

using Unity.Entities;

namespace DOTS.Utilities.PropertiesBlob
{
    public struct PropertiesDataBlobReference : IComponentData
    {
        public BlobAssetReference<PropertiesDataBlob> propertiesBlobReference;
    }
}

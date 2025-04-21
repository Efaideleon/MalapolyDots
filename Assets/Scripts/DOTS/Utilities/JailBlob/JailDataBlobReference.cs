using Unity.Entities;

namespace DOTS.Utilities.JailBlob
{
    public struct JailDataBlobReference : IComponentData
    {
        public BlobAssetReference<JailDataBlob> jailBlobReference;
    }
}

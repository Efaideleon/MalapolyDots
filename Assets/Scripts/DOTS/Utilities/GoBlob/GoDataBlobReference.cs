using Unity.Entities;

namespace DOTS.Utilities.GoBlob
{
    public struct GoDataBlobReference : IComponentData
    {
        public BlobAssetReference<GoDataBlob> goBlobReference;
    }
}

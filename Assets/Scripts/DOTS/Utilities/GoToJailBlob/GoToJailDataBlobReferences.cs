using Unity.Entities;

namespace DOTS.Utilities.GoToJailBlob
{
    public struct GoToJailDataBlobReference : IComponentData
    {
        public BlobAssetReference<GoToJailDataBlob> goToJailBlobReference;
    }
}

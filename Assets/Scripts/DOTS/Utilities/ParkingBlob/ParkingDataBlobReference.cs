using Unity.Entities;

namespace DOTS.Utilities.ParkingBlob
{
    public struct ParkingDataBlobReference : IComponentData
    {
        public BlobAssetReference<ParkingDataBlob> parkingBlobReference;
    }
}

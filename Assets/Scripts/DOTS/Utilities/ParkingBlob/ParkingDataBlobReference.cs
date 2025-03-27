using Unity.Entities;

public struct ParkingDataBlobReference : IComponentData
{
    public BlobAssetReference<ParkingDataBlob> parkingBlobReference;
}

using Unity.Collections;
using Unity.Entities;

public static class ParkingDataBlobBuilder
{
    public static BlobAssetReference<ParkingDataBlob> Create(ParkingData parkingData)
    {
        var builder = new BlobBuilder(Allocator.Temp);

        ref ParkingDataBlob root = ref builder.ConstructRoot<ParkingDataBlob>();

        root.parking.id= parkingData.id;
        root.parking.Name = parkingData.Name;
        root.parking.boardIndex = parkingData.boardIndex;

        BlobAssetReference<ParkingDataBlob> blobReference = builder.CreateBlobAssetReference<ParkingDataBlob>(Allocator.Persistent);
        builder.Dispose();
        return blobReference;
    }
}

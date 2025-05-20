using Unity.Entities;

namespace DOTS.Utilities.ParkingBlob
{
    public struct ParkingDataBlobBuilder
    {
        public static BlobAssetReference<ParkingDataBlob> Create(ParkingData parkingData, IBaker baker)
        {
            return GenericBlobAssetBuilder.CreateBlobAsset(baker, 
                    (BlobBuilder builder, ref ParkingDataBlob root) => 
                    {
                    root.parking.id= parkingData.id;
                    root.parking.Name = parkingData.Name;
                    root.parking.boardIndex = parkingData.boardIndex;
                    });
        }
    }
}

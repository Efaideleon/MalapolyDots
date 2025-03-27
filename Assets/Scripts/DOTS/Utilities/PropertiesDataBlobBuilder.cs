using Unity.Collections;
using Unity.Entities;

public static class PropertiesDataBlobBuilder
{
    public static BlobAssetReference<PropertiesDataBlob> Create(PropertyData[] propertiesData)
    {
        // Builder to help us allocate memory for elements in the blob asset
        // and also allocate memory for the blob asset itself.
        var builder = new BlobBuilder(Allocator.Temp);

        // Helps us get a reference to where we are going to allocate memory to.
        ref PropertiesDataBlob root = ref builder.ConstructRoot<PropertiesDataBlob>();

        // Using the builder to allocate memory to the BlobArray in the Blob Asset reference.
        BlobBuilderArray<FixedPropertyData> propertiesBuilder = builder.Allocate(ref root.properties, propertiesData.Length);

        for (int i = 0; i < propertiesData.Length; i++)
        {
            PropertyData property = propertiesData[i];
            var propertyRentArray = propertiesData[i].rent;

            // Allocate memroy for the rent array in the propertiesBuilder array.
            BlobBuilderArray<int> rentBuilder = builder.Allocate(ref propertiesBuilder[i].rent, propertyRentArray.Length);
            for (int j = 0; j < propertyRentArray.Length; j++)
            {
                rentBuilder[j] = propertyRentArray[j];
            }
            propertiesBuilder[i].id = property.id;
            propertiesBuilder[i].name = property.Name;
            propertiesBuilder[i].boardIndex = property.boardIndex;
            propertiesBuilder[i].price = property.price;
            propertiesBuilder[i].rentWithHotel = property.rentWithHotel;
        }

        // Actually allocating the memory for the BlobAsset and making it persistent.
        BlobAssetReference<PropertiesDataBlob> blobReference = builder.CreateBlobAssetReference<PropertiesDataBlob>(Allocator.Persistent);
        builder.Dispose();
        return blobReference;
    }
}

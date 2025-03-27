using Unity.Collections;
using Unity.Entities;

public static class TreasureDataBlobBuilder
{
    public static BlobAssetReference<TreasureDataBlob> Create(TreasureData[] treasuresData)
    {
        var builder = new BlobBuilder(Allocator.Temp);

        ref TreasureDataBlob root = ref builder.ConstructRoot<TreasureDataBlob>();

        BlobBuilderArray<FixedTreasureData> treasuresBuilder = builder.Allocate(ref root.treasures, treasuresData.Length);

        for (int i = 0; i < treasuresData.Length; i++)
        {
            TreasureData tax = treasuresData[i];
            treasuresBuilder[i].id = tax.id;
            treasuresBuilder[i].Name = tax.Name;
            treasuresBuilder[i].boardIndex = tax.boardIndex;
        }

        BlobAssetReference<TreasureDataBlob> blobReference = builder.CreateBlobAssetReference<TreasureDataBlob>(Allocator.Persistent);
        builder.Dispose();
        return blobReference;
    }
}

using Unity.Collections;
using Unity.Entities;

public static class ChancesDataBlobBuilder
{
    public static BlobAssetReference<ChancesDataBlob> Create(ChanceData[] chancesData)
    {
        var builder = new BlobBuilder(Allocator.Temp);

        ref ChancesDataBlob root = ref builder.ConstructRoot<ChancesDataBlob>();

        BlobBuilderArray<FixedChanceData> chancesBuilder = builder.Allocate(ref root.chances, chancesData.Length);

        for (int i = 0; i < chancesData.Length; i++)
        {
            ChanceData chance = chancesData[i];
            chancesBuilder[i].id = chance.id;
            chancesBuilder[i].Name = chance.Name;
            chancesBuilder[i].boardIndex = chance.boardIndex;
        }

        BlobAssetReference<ChancesDataBlob> blobReference = builder.CreateBlobAssetReference<ChancesDataBlob>(Allocator.Persistent);
        builder.Dispose();
        return blobReference;
    }
}

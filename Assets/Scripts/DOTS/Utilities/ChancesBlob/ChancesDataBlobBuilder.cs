using Unity.Entities;

public static class ChancesDataBlobBuilder
{
    public static BlobAssetReference<ChancesDataBlob> Create(ChanceData[] chancesData, IBaker baker)
    {
        return GenericBlobAssetBuilder.CreateBlobAsset(baker, 
        (BlobBuilder builder, ref ChancesDataBlob root) => 
        {
            BlobBuilderArray<FixedChanceData> chancesBuilder = builder.Allocate(ref root.chances, chancesData.Length);

            for (int i = 0; i < chancesData.Length; i++)
            {
                ChanceData chance = chancesData[i];
                chancesBuilder[i].id = chance.id;
                chancesBuilder[i].Name = chance.Name;
                chancesBuilder[i].boardIndex = chance.boardIndex;
            }
        });
    }
}

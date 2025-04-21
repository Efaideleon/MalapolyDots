using Unity.Entities;

namespace DOTS.Utilities.TreasuresBlob
{
    public static class TreasureDataBlobBuilder
    {
        public static BlobAssetReference<TreasureDataBlob> Create(TreasureData[] treasuresData, IBaker baker)
        {
            return GenericBlobAssetBuilder.CreateBlobAsset(baker, 
                    (BlobBuilder builder, ref TreasureDataBlob root) =>
                    {
                    BlobBuilderArray<FixedTreasureData> treasuresBuilder = builder.Allocate(ref root.treasures, treasuresData.Length);

                    for (int i = 0; i < treasuresData.Length; i++)
                    {
                    TreasureData tax = treasuresData[i];
                    treasuresBuilder[i].id = tax.id;
                    treasuresBuilder[i].Name = tax.Name;
                    treasuresBuilder[i].boardIndex = tax.boardIndex;
                    }
                    });
        }
    }
}

using Unity.Entities;

namespace DOTS.Utilities.ChancesBlob
{
    public struct ChancesDataBlobBuilder
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

                            var chanceActionDataLength = chance.chanceActionData.Length;

                            BlobBuilderArray<FixedChanceActionData> chanceActionBuilder = builder.Allocate(
                                    ref chancesBuilder[i].actionData, chanceActionDataLength
                            );

                            // loop through the chanceCardData here
                            for (int j = 0; j < chance.chanceActionData.Length; j++)
                            {
                                chanceActionBuilder[j].id = chance.chanceActionData[j].id;
                                chanceActionBuilder[j].msg = chance.chanceActionData[j].msg;
                            }
                        }
                    });
        }
    }
}

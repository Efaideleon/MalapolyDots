using Unity.Entities;

namespace DOTS.Utilities.TreasuresBlob
{
    public struct TreasureDataBlobBuilder
    {
        public static BlobAssetReference<TreasureDataBlob> Create(TreasuresData treasuresData, IBaker baker)
        {
            return GenericBlobAssetBuilder.CreateBlobAsset(baker,
                    (BlobBuilder builder, ref TreasureDataBlob root) =>
                    {
                        BlobBuilderArray<FixedTreasureData> treasuresDataBuilder = builder.Allocate(
                                ref root.treasures, treasuresData.treasures.Length);

                        BlobBuilderArray<FixedTreasureCardData> cardsBuilder = builder.Allocate(
                                ref root.cards, treasuresData.cards.Length);

                        for (int i = 0; i < treasuresData.treasures.Length; i++)
                        {
                            TreasureData treasure = treasuresData.treasures[i];
                            treasuresDataBuilder[i].id = treasure.id;
                            treasuresDataBuilder[i].Name = treasure.Name;
                            treasuresDataBuilder[i].boardIndex = treasure.boardIndex;
                        }

                        for (int i = 0; i < treasuresData.cards.Length; i++)
                        {
                            TreasureCardData card = treasuresData.cards[i];
                            cardsBuilder[i].id = card.id;
                            cardsBuilder[i].data = card.data;
                        }
                    });
        }
    }
}

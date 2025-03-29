using Unity.Entities;

public static class TaxesDataBlobBuilder
{
    public static BlobAssetReference<TaxesDataBlob> Create(TaxData[] taxesData, IBaker baker)
    {
        return GenericBlobAssetBuilder.CreateBlobAsset(baker, 
        (BlobBuilder builder, ref TaxesDataBlob root) =>
        {
            BlobBuilderArray<FixedTaxesData> taxesBuilder = builder.Allocate(ref root.taxes, taxesData.Length);

            for (int i = 0; i < taxesData.Length; i++)
            {
            TaxData tax = taxesData[i];
            taxesBuilder[i].id = tax.id;
            taxesBuilder[i].Name = tax.Name;
            taxesBuilder[i].boardIndex = tax.boardIndex;
            }
        });
    }
}

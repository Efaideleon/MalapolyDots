using Unity.Collections;
using Unity.Entities;

public static class TaxesDataBlobBuilder
{
    public static BlobAssetReference<TaxesDataBlob> Create(TaxData[] taxesData)
    {
        var builder = new BlobBuilder(Allocator.Temp);

        ref TaxesDataBlob root = ref builder.ConstructRoot<TaxesDataBlob>();

        BlobBuilderArray<FixedTaxesData> taxesBuilder = builder.Allocate(ref root.taxes, taxesData.Length);

        for (int i = 0; i < taxesData.Length; i++)
        {
            TaxData tax = taxesData[i];
            taxesBuilder[i].id = tax.id;
            taxesBuilder[i].Name = tax.Name;
            taxesBuilder[i].boardIndex = tax.boardIndex;
        }

        BlobAssetReference<TaxesDataBlob> blobReference = builder.CreateBlobAssetReference<TaxesDataBlob>(Allocator.Persistent);
        builder.Dispose();
        return blobReference;
    }
}

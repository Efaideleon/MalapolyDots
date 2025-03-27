using Unity.Collections;
using Unity.Entities;

public struct FixedTaxesData
{
    public int id;
    public FixedString32Bytes Name;
    public int boardIndex;
}

public struct TaxesDataBlob
{
    public BlobArray<FixedTaxesData> taxes;
}

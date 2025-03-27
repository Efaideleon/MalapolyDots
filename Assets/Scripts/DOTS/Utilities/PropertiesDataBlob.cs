using Unity.Collections;
using Unity.Entities;

public struct FixedPropertyData
{
    public int id;
    public FixedString32Bytes name;
    public int boardIndex;
    public int price;
    public BlobArray<int> rent;
    public int rentWithHotel;
}

public struct PropertiesDataBlob
{
    public BlobArray<FixedPropertyData> properties;
}

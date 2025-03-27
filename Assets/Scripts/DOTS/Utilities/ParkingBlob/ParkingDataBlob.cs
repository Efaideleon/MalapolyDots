using Unity.Collections;

public struct FixedParkingData
{
    public int id;
    public FixedString32Bytes Name;
    public int boardIndex;
}

public struct ParkingDataBlob
{
    public FixedParkingData parking;
}

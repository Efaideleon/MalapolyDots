using Unity.Entities;

public struct RentComponent : IComponentData
{
    public BlobArray<int> Value;
}

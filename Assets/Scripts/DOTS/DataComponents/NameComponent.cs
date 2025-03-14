using Unity.Entities;
using Unity.Collections;

public struct NameComponent : IComponentData
{
    public FixedString64Bytes Value;
}


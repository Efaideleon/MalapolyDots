using Unity.Entities;
using Unity.Collections;

public struct NameDataComponent : IComponentData
{
    public FixedString64Bytes Name;
}

public struct PrefabComponent : IComponentData
{
    public Entity prefab;
}

public struct AvocadoDataComponent : IComponentData
{
    public FixedString64Bytes Name;
}

public struct LiraDataComponent : IComponentData
{
    public FixedString64Bytes Name;
}

public struct CoinDataComponent : IComponentData
{
    public FixedString64Bytes Name;
}

public struct MugDataComponent : IComponentData
{
    public FixedString64Bytes Name;
}

public struct BirdDataComponent : IComponentData
{
    public FixedString64Bytes Name;
}

public struct TucTucDataComponent : IComponentData
{
    public FixedString64Bytes Name;
}

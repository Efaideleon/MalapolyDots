using Unity.Collections;
using Unity.Entities;

// Blob Asset?
public struct GameDataComponent : IComponentData
{
    public int NumberOfRounds;
    public int NumberOfPlayers;
}

public struct CharacterSelectedBuffer : IBufferElementData
{
    public FixedString32Bytes Value;
}

public struct PrefabTag : IComponentData
{}

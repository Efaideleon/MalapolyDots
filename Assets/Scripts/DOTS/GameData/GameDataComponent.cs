using Unity.Collections;
using Unity.Entities;

public struct GameDataComponent : IComponentData
{
    public int NumberOfRounds;
    public int NumberOfPlayers;
    public NativeArray<FixedString32Bytes> CharactersSelected;
}

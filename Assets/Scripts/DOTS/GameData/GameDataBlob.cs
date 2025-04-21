using Unity.Entities;

namespace DOTS.GameData
{
    public struct GameDataBlob
    {
        public int NumberOfPlayers;
        public int NumberOfRounds;
        public BlobArray<BlobString> CharactersSelected;
    }
}

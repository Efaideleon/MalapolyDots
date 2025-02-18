using Unity.Entities;

namespace DOTS
{
    public struct GameDataBlob
    {
        public int NumberOfPlayers;
        public int NumberOfRounds;
        public BlobArray<BlobString> CharactersSelected;
    }
}
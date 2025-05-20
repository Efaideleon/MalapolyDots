using Unity.Collections;
using Unity.Entities;

namespace DOTS.Utilities.TreasuresBlob
{
    public struct FixedTreasureData
    {
        public int id;
        public FixedString32Bytes Name;
        public int boardIndex;
    }

    public struct FixedTreasureCardData
    {
        public int id;
        public FixedString64Bytes data;

    }

    public struct TreasureDataBlob
    {
        public BlobArray<FixedTreasureData> treasures;
        public BlobArray<FixedTreasureCardData> cards;
    }
}

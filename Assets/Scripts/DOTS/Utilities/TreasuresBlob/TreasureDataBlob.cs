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

    public struct TreasureDataBlob
    {
        public BlobArray<FixedTreasureData> treasures;
    }
}

using Unity.Collections;
using Unity.Entities;

namespace DOTS.Utilities.ChancesBlob
{
    public struct FixedChanceData
    {
        public int id;
        public FixedString32Bytes Name;
        public int boardIndex;
    }

    public struct ChancesDataBlob
    {
        public BlobArray<FixedChanceData> chances;
    }
}

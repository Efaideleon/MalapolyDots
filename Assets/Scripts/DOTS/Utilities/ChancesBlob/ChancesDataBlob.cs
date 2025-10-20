using Unity.Collections;
using Unity.Entities;

namespace DOTS.Utilities.ChancesBlob
{
    public struct FixedChanceData
    {
        public int id;
        public FixedString32Bytes Name;
        public int boardIndex;
        public BlobArray<FixedChanceActionData> actionData;
    }

    public struct FixedChanceActionData
    {
        public int id;
        public FixedString64Bytes msg;
    }

    public struct ChancesDataBlob
    {
        public BlobArray<FixedChanceData> chances;
    }
}

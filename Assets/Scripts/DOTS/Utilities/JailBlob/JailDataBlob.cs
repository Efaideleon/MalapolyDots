using Unity.Collections;

namespace DOTS.Utilities.JailBlob
{
    public struct FixedJailData
    {
        public int id;
        public FixedString32Bytes Name;
        public int boardIndex;
    }

    public struct JailDataBlob
    {
        public FixedJailData jail;
    }
}

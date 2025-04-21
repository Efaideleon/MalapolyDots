using Unity.Collections;

namespace DOTS.Utilities.GoBlob
{
    public struct FixedGoData
    {
        public int id;
        public FixedString32Bytes Name;
        public int boardIndex;
    }

    public struct GoDataBlob
    {
        public FixedGoData go;
    }
}

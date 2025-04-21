using Unity.Collections;

namespace DOTS.Utilities.GoToJailBlob
{
    public struct FixedGoToJailData
    {
        public int id;
        public FixedString32Bytes Name;
        public int boardIndex;
    }

    public struct GoToJailDataBlob
    {
        public FixedGoToJailData goToJail;
    }
}

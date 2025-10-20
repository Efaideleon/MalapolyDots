using Unity.Collections;
using Unity.Entities;

namespace DOTS.DataComponents
{
    public struct ChanceCardPicked : IComponentData
    {
        public int id;
        public FixedString64Bytes msg;
    }
}

using Unity.Collections;
using Unity.Entities;

namespace DOTS.DataComponents
{
    // TODO: this might be ghost component or do we send it to the client as an rpc?
    public struct ChanceCardPicked : IComponentData
    {
        public int id;
        public FixedString64Bytes msg;
    }
}

using Unity.Collections;
using Unity.Entities;

namespace DOTS.DataComponents
{
    public enum ChanceEffect : byte
    {
        Money,
        BuildingRepairs,
        PropertyIncome,
        PayEachPlayer,
        CollectFromEachPlayer,
        AdvanceToSpace,
        GoToJail,
        JailFreeCard,
        AdvanceToTransport
    }

    // TODO: this might be ghost component or do we send it to the client as an rpc?
    public struct ChanceCardPicked : IComponentData
    {
        public int id;
        public FixedString64Bytes msg;
    }
}

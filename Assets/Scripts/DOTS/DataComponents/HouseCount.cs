using Unity.Entities;
using Unity.NetCode;

namespace DOTS.DataComponents
{
    [GhostComponent]
    public struct HouseCount : IComponentData
    {
        [GhostField]
        public int Value;
    }

    public enum PropertyRentKind : byte { Street, Transport, Utility }

    public struct PropertyRentKindComponent : IComponentData
    {
        public PropertyRentKind Value;
    }

    public struct HousePriceComponent : IComponentData
    {
        public int Value;
    }
}

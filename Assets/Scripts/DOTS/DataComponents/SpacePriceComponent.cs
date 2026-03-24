using Unity.Entities;
using Unity.NetCode;

namespace DOTS.DataComponents
{
    public struct PriceComponent : IComponentData
    {
        public int Value;
    }

    [GhostComponent]
    public struct GhostPriceComponent : IComponentData
    {
        [GhostField]
        public int Value;
    }
}

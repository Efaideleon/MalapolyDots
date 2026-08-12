using Unity.Entities;
using Unity.NetCode;

namespace DOTS.DataComponents
{
    [GhostComponent]
    public struct GhostRentComponent : IComponentData
    {
        [GhostField]
        public int Value;
    }
}

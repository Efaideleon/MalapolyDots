using Unity.Entities;
using Unity.NetCode;

namespace DOTS.DataComponents
{
    [GhostComponent]
    public struct MonopolyFlagComponent : IComponentData
    {
        [GhostField]
        public bool Value;
    }
}

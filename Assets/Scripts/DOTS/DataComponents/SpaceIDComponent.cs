using Unity.Entities;
using Unity.NetCode;

namespace DOTS.DataComponents
{
    [GhostComponent]
    public struct SpaceIDComponent : IComponentData
    {
        [GhostField]
        public int Value;
    }
}

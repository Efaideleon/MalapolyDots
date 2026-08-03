using Unity.Entities;
using Unity.NetCode;

namespace DOTS.DataComponents
{
    [GhostComponent]
    public struct ForSaleComponent : IComponentData
    {
        [GhostField]
        public Entity entity;
    }
}

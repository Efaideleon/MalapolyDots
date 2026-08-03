using Unity.Entities;
using Unity.NetCode;

namespace DOTS.DataComponents
{
    [GhostComponent]
    public struct OwnerByEntityComponent : IComponentData
    {
        [GhostField]
        public Entity Entity;
    }
}

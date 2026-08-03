using Unity.Entities;
using Unity.NetCode;

namespace DOTS.DataComponents
{
    [GhostComponent]
    public struct OwnerComponent : IComponentData
    {
        [GhostField]
        public int ID;
    }
}

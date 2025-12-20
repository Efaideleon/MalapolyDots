using Unity.Entities;

namespace DOTS.DataComponents
{
    public struct OwnerByEntityComponent : IComponentData
    {
        public Entity Entity;
    }
}

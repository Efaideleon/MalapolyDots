using Unity.Entities;

namespace DOTS.DataComponents
{
    public struct CurrentPlayerComponent : IComponentData
    {
        public Entity entity;
    }
}

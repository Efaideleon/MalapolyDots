using Unity.Entities;

namespace DOTS.DataComponents
{
    public struct LastPropertyInteracted : IComponentData
    {
        public Entity entity;
    }
}

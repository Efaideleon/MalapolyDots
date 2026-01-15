using Unity.Entities;

namespace DOTS.EventBuses
{
    public struct UIButtonDirtyFlag : IComponentData
    {
        public bool Value;
    }
}

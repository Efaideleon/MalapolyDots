using Unity.Entities;
using Unity.Collections;

namespace DOTS.DataComponents
{
    public struct NameComponent : IComponentData
    {
        public FixedString64Bytes Value;
    }

}

using Unity.Entities;
using Unity.NetCode;

namespace DOTS.DataComponents
{
    public struct MoneyComponent : IComponentData
    {
        public int Value;
    }
}

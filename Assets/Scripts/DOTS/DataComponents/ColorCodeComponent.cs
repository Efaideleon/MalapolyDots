using Unity.Entities;

namespace DOTS.DataComponents
{
    public enum PropertyColor
    {
        White,
        Brown,
        LightBlue,
        Purple,
        Orange,
        Red,
        Yellow,
        Green,
        Blue
    }

    public struct ColorCodeComponent : IComponentData
    {
        public PropertyColor Value;
    }
}

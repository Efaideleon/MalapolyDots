using Unity.Entities;

namespace DOTS.DataComponents
{
    public enum SpaceType
    {
        Property,
        Treasure,
        Tax,
        Chance,
        Parking,
        Go,
        GoToJail,
        Jail,
        Default
    }

    public struct SpaceTypeComponent : IComponentData
    {
        public SpaceType Value;
    }
}

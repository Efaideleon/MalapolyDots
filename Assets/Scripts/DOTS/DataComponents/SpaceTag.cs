using Unity.Entities;

public enum SpaceTypeEnum
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
    public SpaceTypeEnum Value;
}

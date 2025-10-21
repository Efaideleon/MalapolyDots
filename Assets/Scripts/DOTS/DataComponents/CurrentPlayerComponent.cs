using Unity.Entities;

namespace DOTS.DataComponents
{
    /// <summary>
    /// The entity for the player currently active in the game.
    /// </summary>
    public struct CurrentPlayerComponent : IComponentData
    {
        public Entity entity;
    }
}

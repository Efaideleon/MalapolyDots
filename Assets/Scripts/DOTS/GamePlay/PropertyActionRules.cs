using Assets.Scripts.DOTS.Characters;
using Unity.Entities;

namespace DOTS.GamePlay
{
    public static class PropertyActionRules
    {
        public static bool CanOpen(GameState gameState, MoveState movement, Entity landedProperty, Entity tappedProperty)
        {
            return gameState != GameState.Walking && gameState != GameState.GameOver &&
                movement == MoveState.Idle && tappedProperty != Entity.Null && tappedProperty == landedProperty;
        }
    }
}

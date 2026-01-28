using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay.NetcodeSystems.Gameplay.Authorings
{
    public class GamePhaseGhostAuthoring : MonoBehaviour
    {
        public class GamePhaseGhostBaker : Baker<GamePhaseGhostAuthoring>
        {
            public override void Bake(GamePhaseGhostAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new GamePhaseGhostComponent { GamePhase = GamePhase.Default });
            }
        }
    }

    [GhostComponent]
    public struct GamePhaseGhostComponent : IComponentData
    {
        [GhostField]
        public GamePhase GamePhase;
    }

    public enum GamePhase
    {
        Default,
        StartMenu,
        Game
    }
}

using DOTS.GamePlay;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Assets.Scripts.DOTS.GamePlay
{
    public class GameStateGhostAuthoring : MonoBehaviour
    {
        public class GameStateGhostBaker : Baker<GameStateGhostAuthoring>
        {
            public override void Bake(GameStateGhostAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new GameStateComponent { State = GameState.Rolling });
            }
        }
    }

    [GhostComponent]
    public struct GameStateComponent : IComponentData
    {
        [GhostField]
        public GameState State;
    }
}

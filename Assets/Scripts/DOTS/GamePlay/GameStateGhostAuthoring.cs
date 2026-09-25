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
                AddComponent(entity, new GameStateComponent { State = GameState.Rolling, AllPlacesInstantiated = false });
            }
        }
    }

    [GhostComponent]
    public struct GameStateComponent : IComponentData
    {
        [GhostField]
        public GameState State;

        // One server-authored event per completed move. Turn changes retain this identity.
        [GhostField] public uint LandingSequence;
        [GhostField] public Entity LandingPlayer;
        [GhostField] public Entity LandingSpace;

        public bool HasUnseenLanding(Entity player, uint lastPresentedSequence)
        {
            return State == GameState.Landing && LandingSequence != 0 &&
                LandingSequence != lastPresentedSequence && player != Entity.Null &&
                LandingPlayer == player && LandingSpace != Entity.Null;
        }

        [GhostField]
        public bool AllPlacesInstantiated;

        [GhostField]
        public int WinnerNetworkId;

        [GhostField] public int RoundLimit;
        [GhostField] public int CompletedRounds;
        [GhostField] public bool EndedByRoundLimit;
    }
}

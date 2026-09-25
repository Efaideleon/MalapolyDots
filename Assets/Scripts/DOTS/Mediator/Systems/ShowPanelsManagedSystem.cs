using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GamePlay;
using Unity.Entities;
using Unity.NetCode;

#nullable enable
namespace DOTS.Mediator.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct ShowPanelsManagedSystem : ISystem
    {
        private uint lastPresentedLanding;
        public ComponentLookup<GameStateComponent> gameStateLookup;
        public ComponentLookup<SpaceLandedOn> spaceLandedOnLookup;
        public ComponentLookup<SpaceTypeComponent> spaceTypeLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<SpaceLandedOn>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GhostDataLoadedTag>();
            state.RequireForUpdate<PanelControllersManagerComponent>();
            state.RequireForUpdate<GameScreenInitializedFlag>();
            state.RequireForUpdate<SpaceTypeComponent>();

            gameStateLookup = SystemAPI.GetComponentLookup<GameStateComponent>();
            spaceLandedOnLookup = SystemAPI.GetComponentLookup<SpaceLandedOn>();
            spaceTypeLookup = SystemAPI.GetComponentLookup<SpaceTypeComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            gameStateLookup.Update(ref state);
            spaceLandedOnLookup.Update(ref state);
            spaceTypeLookup.Update(ref state);

            Entity gameStateEntity = SystemAPI.GetSingletonEntity<GameStateComponent>();
            Entity activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;

            if (activePlayerEntity == Entity.Null) return;

            var landing = gameStateLookup[gameStateEntity];
            if (!landing.HasUnseenLanding(activePlayerEntity, lastPresentedLanding)) return;

            foreach (var (spaceLandedOn, player) in SystemAPI.Query<RefRO<SpaceLandedOn>>().WithAll<GhostOwnerIsLocal, ActivePlayer>().WithEntityAccess())
            {
                GameState gameState = gameStateLookup[gameStateEntity].State;
                if (gameState != GameState.Landing) return;

                if (player != landing.LandingPlayer || spaceLandedOn.ValueRO.entity != landing.LandingSpace) continue;

                // The place where the player lands on.
                Entity placeEntity = spaceLandedOn.ValueRO.entity;
                UnityEngine.Debug.Log($"[ShowPanelsManagedSystem] | placeEntity : {placeEntity}");

                // The place type where the player is at.
                if (!spaceTypeLookup.HasComponent(placeEntity)) return;
                SpaceType spaceType = spaceTypeLookup[placeEntity].Value;
                // Card popups are driven by new per-player draws, not shared game-state changes.
                if (spaceType == SpaceType.Chance || spaceType == SpaceType.Treasure) return;

                // Get the panels controllers registry.
                var controllersManager = SystemAPI.ManagedAPI.GetSingleton<PanelControllersManagerComponent>().Manager;
                if (controllersManager == null) return;

                // Show the respective panel based on type.
                lastPresentedLanding = landing.LandingSequence;
                controllersManager.Show(spaceType);
            }
        }
    }
}

using DOTS.Characters;
using DOTS.DataComponents;
using DOTS.GamePlay;
using Unity.Entities;

#nullable enable
namespace DOTS.Mediator.Systems
{
    public partial struct ShowPanelsManagedSystem : ISystem
    {
        public ComponentLookup<GameStateComponent> gameStateLookup;
        public ComponentLookup<SpaceLandedOn> spaceLandedOnLookup;
        public ComponentLookup<SpaceTypeComponent> spaceTypeLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<SpaceLandedOn>();
            state.RequireForUpdate<CurrentActivePlayer>();
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

            if (!gameStateLookup.DidChange(gameStateEntity, state.LastSystemVersion)) return;
            if (!spaceLandedOnLookup.HasComponent(activePlayerEntity)) return;

            GameState gameState = gameStateLookup[gameStateEntity].State;
            if (gameState != GameState.Landing) return;

            // The place where the player lands on.
            Entity placeEntity = spaceLandedOnLookup[activePlayerEntity].entity;

            // The place type where the player is at.
            SpaceType spaceType = spaceTypeLookup[placeEntity].Value;
            if (!spaceTypeLookup.HasComponent(placeEntity)) return;

            // Get the panels controllers registry.
            var controllersManager = SystemAPI.ManagedAPI.GetSingleton<PanelControllersManagerComponent>().Manager;
            if (controllersManager == null) return;

            // Show the respective panel based on type.
            controllersManager.Show(spaceType);
        }
    }
}

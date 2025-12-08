using DOTS.Characters;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Mediator.Systems.PayTaxSystems
{
    public struct ShowPayTaxPanelBuffer : IBufferElementData
    { }

    [BurstCompile]
    public partial struct PayTaxPanelPopupSystem : ISystem
    {
        public ComponentLookup<SpaceLandedOn> spaceLandedOnLookup;
        public ComponentLookup<GameStateComponent> gameStateLookup;
        public ComponentLookup<TaxSpaceTag> taxSpaceLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<ShowPayTaxPanelBuffer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<TaxSpaceTag>();
            state.RequireForUpdate<CurrentActivePlayer>();

            spaceLandedOnLookup = SystemAPI.GetComponentLookup<SpaceLandedOn>(true);
            gameStateLookup = SystemAPI.GetComponentLookup<GameStateComponent>(true);
            taxSpaceLookup = SystemAPI.GetComponentLookup<TaxSpaceTag>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            gameStateLookup.Update(ref state);
            spaceLandedOnLookup.Update(ref state);
            taxSpaceLookup.Update(ref state);

            var gameStateEntity = SystemAPI.GetSingletonEntity<GameStateComponent>();

            if (!gameStateLookup.HasComponent(gameStateEntity)) return;

            if (!gameStateLookup.DidChange(gameStateEntity, state.LastSystemVersion)) return;

            if (gameStateLookup[gameStateEntity].State != GameState.Landing) return;

            var activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;

            if (!spaceLandedOnLookup.HasComponent(activePlayerEntity)) return;

            var landedOnEntity = spaceLandedOnLookup[activePlayerEntity].entity;

            if (!taxSpaceLookup.HasComponent(landedOnEntity)) return;

            SystemAPI.GetSingletonBuffer<ShowPayTaxPanelBuffer>().Add(new ShowPayTaxPanelBuffer { });
        }
    }
}

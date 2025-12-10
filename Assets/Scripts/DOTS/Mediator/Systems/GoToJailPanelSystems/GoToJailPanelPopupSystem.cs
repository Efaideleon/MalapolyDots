using DOTS.Characters;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;

namespace DOTS.Mediator.Systems
{
    public struct ShowGoToJailPanelBuffer : IBufferElementData
    { }

    [BurstCompile]
    public partial struct GoToJailPanelPopupSystem : ISystem
    {
        public ComponentLookup<GameStateComponent> gameStateLookup;
        public ComponentLookup<SpaceLandedOn> spaceLandedOnLookup;
        public ComponentLookup<GoToJailTag> goToJailLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingletonBuffer<ShowGoToJailPanelBuffer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<SpaceLandedOn>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GoToJailTag>();

            gameStateLookup = SystemAPI.GetComponentLookup<GameStateComponent>();
            spaceLandedOnLookup = SystemAPI.GetComponentLookup<SpaceLandedOn>();
            goToJailLookup = SystemAPI.GetComponentLookup<GoToJailTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            gameStateLookup.Update(ref state);
            spaceLandedOnLookup.Update(ref state);
            goToJailLookup.Update(ref state);

            Entity gameStateEntity = SystemAPI.GetSingletonEntity<GameStateComponent>();
            Entity activePlayerEntity = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;

            if (gameStateLookup.HasComponent(gameStateEntity) && gameStateLookup.DidChange(gameStateEntity, state.LastSystemVersion))
            {
                if (gameStateLookup[gameStateEntity].State == GameState.Landing)
                {
                    if (spaceLandedOnLookup.HasComponent(activePlayerEntity))
                    {
                        if (goToJailLookup.HasComponent(spaceLandedOnLookup[activePlayerEntity].entity))
                        {
                            SystemAPI.GetSingletonBuffer<ShowGoToJailPanelBuffer>().Add(new ShowGoToJailPanelBuffer { });
                        }
                    }
                }
            }
        }
    }
}

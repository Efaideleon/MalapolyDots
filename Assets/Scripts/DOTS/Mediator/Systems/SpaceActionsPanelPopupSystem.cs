using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.Mediator.Systems
{
    public struct ShowActionsPanelBuffer : IBufferElementData
    { }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [BurstCompile]
    public partial struct SpaceActionsPanelPopupSystem : ISystem
    {
        public ComponentLookup<GameStateComponent> gameStateLookup;
        public ComponentLookup<PropertySpaceTag> propertySpaceLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<PropertySpaceTag>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<SpaceLandedOn>();
            state.RequireForUpdate<GhostDataLoadedTag>();

            state.EntityManager.CreateSingletonBuffer<ShowActionsPanelBuffer>();

            gameStateLookup = SystemAPI.GetComponentLookup<GameStateComponent>();
            propertySpaceLookup = SystemAPI.GetComponentLookup<PropertySpaceTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            gameStateLookup.Update(ref state);
            propertySpaceLookup.Update(ref state);

            // TODO: this should only run for the current active player.
            var gameStateEntity = SystemAPI.GetSingletonEntity<GameStateComponent>();
            if (gameStateLookup.HasComponent(gameStateEntity) && gameStateLookup.DidChange(gameStateEntity, state.LastSystemVersion))
            {
                if (gameStateLookup[gameStateEntity].State == GameState.Landing)
                {
                    foreach (var spaceLandedOn in SystemAPI.Query<RefRO<SpaceLandedOn>>().WithAll<ActivePlayer, GhostOwnerIsLocal>())
                    {
                        if (propertySpaceLookup.HasComponent(spaceLandedOn.ValueRO.entity))
                        {
                            UnityEngine.Debug.Log($"[SpaceActionsPanelPopupSystem] | sending event to show the actions panel.");
                            SystemAPI.GetSingletonBuffer<ShowActionsPanelBuffer>().Add(new ShowActionsPanelBuffer { });
                        }
                    }
                }
            }
        }
    }
}

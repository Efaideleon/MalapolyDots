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
    {
        public Entity Player;
        public Entity Property;
        public uint LandingSequence;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [BurstCompile]
    public partial struct SpaceActionsPanelPopupSystem : ISystem
    {
        private uint lastPresentedLanding;
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

            var game = SystemAPI.GetSingleton<GameStateComponent>();
            var player = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            if (!game.HasUnseenLanding(player, lastPresentedLanding)) return;

            foreach (var (space, entity) in SystemAPI.Query<RefRO<SpaceLandedOn>>()
                         .WithAll<ActivePlayer, GhostOwnerIsLocal>().WithEntityAccess())
            {
                if (entity != player || space.ValueRO.entity != game.LandingSpace ||
                    !propertySpaceLookup.HasComponent(game.LandingSpace)) continue;
                SystemAPI.GetSingletonBuffer<ShowActionsPanelBuffer>().Add(new ShowActionsPanelBuffer
                {
                    Player = player, Property = game.LandingSpace, LandingSequence = game.LandingSequence
                });
                lastPresentedLanding = game.LandingSequence;
            }
        }
    }
}

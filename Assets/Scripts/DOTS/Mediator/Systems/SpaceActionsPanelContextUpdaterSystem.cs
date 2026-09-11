using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.UI.Controllers;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.Mediator.Systems
{
    public struct SpaceActionsPanelContextComponent : IComponentData
    {
        public SpaceActionsPanelContext Value;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct SpaceActionsPanelContextUpdaterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new SpaceActionsPanelContextComponent());
            state.RequireForUpdate<LastPropertyInteracted>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<NetworkId>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var context = new SpaceActionsPanelContext();
            var property = SystemAPI.GetSingleton<LastPropertyInteracted>().entity;
            var player = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            int localId = SystemAPI.GetSingleton<NetworkId>().Value;
            if (SystemAPI.HasComponent<PropertySpaceTag>(property) && SystemAPI.HasComponent<GhostOwner>(player))
            {
                context.IsPlayerOwner = SystemAPI.GetComponent<OwnerComponent>(property).ID == localId &&
                    SystemAPI.GetComponent<GhostOwner>(player).NetworkId == localId &&
                    SystemAPI.GetSingleton<GameStateComponent>().State != GameState.Walking &&
                    SystemAPI.GetSingleton<GameStateComponent>().State != GameState.GameOver;
                context.HasMonopoly = SystemAPI.GetComponent<MonopolyFlagComponent>(property).Value;
            }
            var current = SystemAPI.GetSingletonRW<SpaceActionsPanelContextComponent>();
            if (current.ValueRO.Value.IsPlayerOwner != context.IsPlayerOwner || current.ValueRO.Value.HasMonopoly != context.HasMonopoly)
                current.ValueRW.Value = context;
        }
    }
}

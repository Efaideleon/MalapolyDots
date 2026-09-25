using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Constants;
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
    [UpdateAfter(typeof(PurchaseHousePanelUpdaterSystem))]
    public partial struct SpaceActionsPanelContextUpdaterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new SpaceActionsPanelContextComponent());
            state.RequireForUpdate<LastPropertyInteracted>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<PurhcaseHousePanelContextComponent>();
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
                var purchase = SystemAPI.GetSingleton<PurhcaseHousePanelContextComponent>().Value;
                bool canBuild = context.IsPlayerOwner && context.HasMonopoly && purchase.MaxPurchasable > 0 &&
                    purchase.PropertyId == SystemAPI.GetComponent<SpaceIDComponent>(property).Value &&
                    SystemAPI.GetSingleton<GameStateComponent>().AllPlacesInstantiated;
                context.CanBuyHouse = canBuild && purchase.HousesOwned >= 0 && purchase.HousesOwned < 4;
                context.CanBuyHotel = canBuild && purchase.HousesOwned == 4;
                int ownerId = SystemAPI.GetComponent<OwnerComponent>(property).ID;
                context.CanBuyProperty = ownerId == PropertyConstants.Vacant &&
                    SystemAPI.GetComponent<GhostOwner>(player).NetworkId == localId &&
                    SystemAPI.GetSingleton<GameStateComponent>().AllPlacesInstantiated &&
                    SystemAPI.GetSingleton<GameStateComponent>().State != GameState.Walking &&
                    SystemAPI.GetSingleton<GameStateComponent>().State != GameState.GameOver &&
                    SystemAPI.HasComponent<PlayerMovementState>(player) &&
                    SystemAPI.GetComponent<PlayerMovementState>(player).Value == MoveState.Idle &&
                    SystemAPI.HasComponent<SpaceLandedOn>(player) &&
                    SystemAPI.GetComponent<SpaceLandedOn>(player).entity == property &&
                    SystemAPI.HasComponent<PriceComponent>(property) &&
                    SystemAPI.GetComponent<PriceComponent>(property).Value > 0 &&
                    SystemAPI.HasComponent<GhostMoneyComponet>(player) &&
                    SystemAPI.GetComponent<GhostMoneyComponet>(player).Value >= SystemAPI.GetComponent<PriceComponent>(property).Value;
                context.MustPayRent = ownerId > 0 && ownerId != localId &&
                    SystemAPI.GetComponent<GhostOwner>(player).NetworkId == localId &&
                    SystemAPI.GetSingleton<GameStateComponent>().State == GameState.Landing &&
                    SystemAPI.HasComponent<SpaceLandedOn>(player) &&
                    SystemAPI.GetComponent<SpaceLandedOn>(player).entity == property &&
                    SystemAPI.HasComponent<LandingPaymentResolved>(player) &&
                    !SystemAPI.GetComponent<LandingPaymentResolved>(player).Value;

            }
            var current = SystemAPI.GetSingletonRW<SpaceActionsPanelContextComponent>();
            if (current.ValueRO.Value.IsPlayerOwner != context.IsPlayerOwner || current.ValueRO.Value.HasMonopoly != context.HasMonopoly ||
                current.ValueRO.Value.CanBuyHouse != context.CanBuyHouse || current.ValueRO.Value.CanBuyHotel != context.CanBuyHotel ||
                current.ValueRO.Value.MustPayRent != context.MustPayRent || current.ValueRO.Value.CanBuyProperty != context.CanBuyProperty)
                current.ValueRW.Value = context;
        }
    }
}

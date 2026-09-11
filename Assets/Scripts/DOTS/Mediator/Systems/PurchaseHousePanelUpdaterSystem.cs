using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.DataComponents;
using DOTS.GamePlay;
using DOTS.GameSpaces;
using DOTS.UI.Panels;
using Assets.Scripts.DOTS.UI.Controllers;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace DOTS.Mediator.Systems
{
    public struct PurhcaseHousePanelContextComponent : IComponentData
    {
        public PurchaseHousePanelContext Value;
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct PurchaseHousePanelUpdaterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.EntityManager.CreateSingleton(new PurhcaseHousePanelContextComponent());
            state.RequireForUpdate<LastPropertyInteracted>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<NetworkId>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var context = new PurchaseHousePanelContext();
            var property = SystemAPI.GetSingleton<LastPropertyInteracted>().entity;
            var player = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            int localId = SystemAPI.GetSingleton<NetworkId>().Value;
            if (SystemAPI.HasComponent<PropertySpaceTag>(property))
            {
                context.PropertyId = SystemAPI.GetComponent<SpaceIDComponent>(property).Value;
                context.Name = SystemAPI.GetComponent<NameComponent>(property).Value;
                context.HousesOwned = SystemAPI.GetComponent<HouseCount>(property).Value;
                context.Price = SystemAPI.GetComponent<HousePriceComponent>(property).Value;
                if (context.Price > 0 && SystemAPI.HasComponent<GhostMoneyComponet>(player) &&
                    SystemAPI.HasComponent<GhostOwner>(player) && SystemAPI.GetComponent<GhostOwner>(player).NetworkId == localId &&
                    SystemAPI.GetComponent<OwnerComponent>(property).ID == localId &&
                    SystemAPI.GetComponent<MonopolyFlagComponent>(property).Value &&
                    SystemAPI.GetSingleton<GameStateComponent>().State != GameState.Walking &&
                    SystemAPI.GetSingleton<GameStateComponent>().State != GameState.GameOver)
                {
                    context.MaxPurchasable = math.max(0, math.min(context.HousesOwned == 4 ? 1 : math.max(0, 4 - context.HousesOwned),
                        SystemAPI.GetComponent<GhostMoneyComponet>(player).Value / context.Price));
                }
            }
            var current = SystemAPI.GetSingletonRW<PurhcaseHousePanelContextComponent>();
            var previous = current.ValueRO.Value;
            if (previous.PropertyId != context.PropertyId || previous.Name != context.Name || previous.HousesOwned != context.HousesOwned ||
                previous.Price != context.Price || previous.MaxPurchasable != context.MaxPurchasable)
                current.ValueRW.Value = context;
        }
    }
}

namespace DOTS.Mediator.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial struct TaxAmountPanelUpdaterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PanelControllerService>();
            state.RequireForUpdate<CurrentActivePlayer>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var player = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            if (!SystemAPI.HasComponent<SpaceLandedOn>(player)) return;
            var space = SystemAPI.GetComponent<SpaceLandedOn>(player).entity;
            if (!SystemAPI.HasComponent<TaxAmountComponent>(space)) return;
            var service = SystemAPI.ManagedAPI.GetSingleton<PanelControllerService>();
            if (service.TryGet<DOTS.UI.Controllers.PayTaxPanelController>(out var panel))
            {
                int amount = SystemAPI.GetComponent<TaxAmountComponent>(space).Value;
                panel.Context = new DOTS.UI.Controllers.PayTaxPanelContext { Amount = amount };
                panel.Panel.AmountLabel.text = amount.ToString("N0");
            }
        }
    }
}

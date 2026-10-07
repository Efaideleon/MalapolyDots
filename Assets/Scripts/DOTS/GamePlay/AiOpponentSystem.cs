using Assets.Common;
using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.Mediator;
using DOTS.Constants;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Collections;
using Unity.Entities;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(GamePlaySystem))]
    [UpdateAfter(typeof(BankruptcySystem))]
    [UpdateAfter(typeof(ChangeTurnSystem))]
    [UpdateAfter(typeof(MatchCompletionSystem))]
    public partial class AiOpponentSystem : SystemBase
    {
        EntityQuery opponents, properties, offers;

        protected override void OnCreate()
        {
            RequireForUpdate<GameStateComponent>();
            RequireForUpdate<CurrentActivePlayer>();
            opponents = GetEntityQuery(typeof(AiOpponent), typeof(GhostMoneyComponet), typeof(BankruptPlayer));
            properties = GetEntityQuery(typeof(OwnerComponent), typeof(OwnerByEntityComponent), typeof(SpaceIDComponent));
            offers = GetEntityQuery(typeof(AiTradeOffer));
        }

        protected override void OnUpdate()
        {
            if (SoloSession.Paused) return;
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            if (!game.AllPlacesInstantiated || game.State == GameState.GameOver) return;
            double now = SystemAPI.Time.ElapsedTime;
            using var deeds = properties.ToEntityArray(Allocator.Temp);
            using var inbox = offers.ToEntityArray(Allocator.Temp);
            foreach (var entity in inbox)
            {
                var offer = EntityManager.GetComponentData<AiTradeOffer>(entity);
                EntityManager.DestroyEntity(entity);
                if (!EntityManager.HasComponent<AiOpponent>(offer.Buyer) ||
                    EntityManager.GetComponentData<BankruptPlayer>(offer.Buyer).Value) continue;
                Entity deed = Entity.Null;
                foreach (var candidate in deeds)
                    if (EntityManager.GetComponentData<SpaceIDComponent>(candidate).Value == offer.PropertyId) { deed = candidate; break; }
                int value = EntityManager.HasComponent<GhostPriceComponent>(deed) ? EntityManager.GetComponentData<GhostPriceComponent>(deed).Value : 0;
                int cash = EntityManager.GetComponentData<GhostMoneyComponet>(offer.Buyer).Value;
                bool accept = AiStrategy.AcceptOffer(cash, offer.Price, value, AiStrategy.CompletesSet(EntityManager, deed, offer.Buyer, deeds));
                Queue(offer.Buyer, new AssetTradeRpc { Action = accept ? AssetAction.Accept : AssetAction.Decline, OfferId = offer.OfferId });
            }

            // A debtor can raise money even when another player's turn is waiting.
            using var bots = opponents.ToEntityArray(Allocator.Temp);
            foreach (var player in bots)
            {
                var bankruptcy = EntityManager.GetComponentData<BankruptPlayer>(player);
                if (bankruptcy.Value || bankruptcy.Declared || EntityManager.GetComponentData<GhostMoneyComponet>(player).Value >= 0) continue;
                if (game.State == GameState.Walking) return;
                var brain = EntityManager.GetComponentData<AiOpponent>(player);
                if (now < brain.NextDecision) return;
                brain.NextDecision = now + 0.8;
                EntityManager.SetComponentData(player, brain);
                RecoverDebt(player, deeds);
                return;
            }
            if (BankruptcyRules.HasDebt(EntityManager)) return;
            var active = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
            if (!EntityManager.HasComponent<AiOpponent>(active)) return;
            var status = EntityManager.GetComponentData<BankruptPlayer>(active);
            if (status.Value || status.Declared) return;
            var ai = EntityManager.GetComponentData<AiOpponent>(active);
            if (ai.ObservedState != (int)game.State)
            {
                ai.ObservedState = (int)game.State;
                ai.NextDecision = now + (game.State == GameState.Landing ? 1.5 : 0.8);
                if (game.State == GameState.Rolling) ai.BuildingsThisTurn = 0;
                EntityManager.SetComponentData(active, ai);
                return;
            }
            if (now < ai.NextDecision || game.State == GameState.Walking) return;
            ai.NextDecision = now + 0.8;
            EntityManager.SetComponentData(active, ai);
            if (game.State == GameState.Rolling)
            {
                if (EntityManager.HasComponent<JailState>(active) && EntityManager.GetComponentData<JailState>(active).InJail)
                    Queue(active, new ChangeTurnRpc());
                else if (!SystemAPI.TryGetSingleton<PendingDiceRoll>(out var pending) || !pending.Active)
                    Queue(active, new RollEventRpc());
                return;
            }
            if (!EntityManager.HasComponent<SpaceLandedOn>(active)) return;
            var space = EntityManager.GetComponentData<SpaceLandedOn>(active).entity;
            if (EntityManager.HasComponent<GoToJailTag>(space)) { Queue(active, new GoToJailRpc()); return; }
            bool paid = EntityManager.HasComponent<LandingPaymentResolved>(active) && EntityManager.GetComponentData<LandingPaymentResolved>(active).Value;
            if (!paid && EntityManager.HasComponent<TaxSpaceTag>(space)) { Queue(active, new PayTaxesRpc()); return; }
            if (!paid && EntityManager.HasComponent<OwnerByEntityComponent>(space))
            {
                var owner = EntityManager.GetComponentData<OwnerByEntityComponent>(space).Entity;
                if (owner != Entity.Null && owner != active) { Queue(active, new PayRentRpc()); return; }
            }
            if (EntityManager.HasComponent<OwnerComponent>(space) && EntityManager.HasComponent<PriceComponent>(space) &&
                EntityManager.GetComponentData<OwnerComponent>(space).ID == PropertyConstants.Vacant &&
                AiStrategy.BuyDeed(EntityManager.GetComponentData<GhostMoneyComponet>(active).Value,
                    EntityManager.GetComponentData<PriceComponent>(space).Value, AiStrategy.CompletesSet(EntityManager, space, active, deeds)))
            {
                Queue(active, new PurchasePropertyEventRpc { ID = EntityManager.GetComponentData<SpaceIDComponent>(space).Value });
                return;
            }
            if (ai.BuildingsThisTurn < 2 && Build(active, deeds))
            {
                ai.BuildingsThisTurn++;
                EntityManager.SetComponentData(active, ai);
                return;
            }
            Queue(active, new ChangeTurnRpc());
        }

        void RecoverDebt(Entity player, NativeArray<Entity> deeds)
        {
            foreach (var deed in deeds)
            {
                if (EntityManager.GetComponentData<OwnerByEntityComponent>(deed).Entity != player ||
                    !EntityManager.HasComponent<HouseCount>(deed) || EntityManager.GetComponentData<HouseCount>(deed).Value <= 0) continue;
                Queue(player, new AssetTradeRpc { PropertyId = EntityManager.GetComponentData<SpaceIDComponent>(deed).Value,
                    Action = AssetTradingRules.BuildingError(EntityManager, deed, deeds, false) == null ? AssetAction.SellBuilding : AssetAction.SellGroupBuildings });
                return;
            }
            Entity cheapest = Entity.Null;
            int cheapestPrice = int.MaxValue;
            foreach (var deed in deeds)
            {
                if (EntityManager.GetComponentData<OwnerByEntityComponent>(deed).Entity != player ||
                    !EntityManager.HasComponent<GhostPriceComponent>(deed) || AssetTradingRules.HasBuildings(EntityManager, deed, deeds)) continue;
                int price = EntityManager.GetComponentData<GhostPriceComponent>(deed).Value;
                if (price > 1 && price < cheapestPrice) { cheapest = deed; cheapestPrice = price; }
            }
            Queue(player, cheapest != Entity.Null ? new AssetTradeRpc { Action = AssetAction.SellPropertyToBank,
                PropertyId = EntityManager.GetComponentData<SpaceIDComponent>(cheapest).Value } : new AssetTradeRpc { Action = AssetAction.DeclareBankruptcy });
        }

        bool Build(Entity player, NativeArray<Entity> deeds)
        {
            int cash = EntityManager.GetComponentData<GhostMoneyComponet>(player).Value;
            foreach (var deed in deeds)
            {
                if (EntityManager.GetComponentData<OwnerByEntityComponent>(deed).Entity != player ||
                    AssetTradingRules.BuildingError(EntityManager, deed, deeds, true) != null) continue;
                if ((long)cash - EntityManager.GetComponentData<HousePriceComponent>(deed).Value < 300) continue;
                Queue(player, new AssetTradeRpc { Action = AssetAction.BuyBuilding, PropertyId = EntityManager.GetComponentData<SpaceIDComponent>(deed).Value });
                return true;
            }
            return false;
        }

        void Queue<T>(Entity player, T action) where T : unmanaged, Unity.NetCode.IRpcCommand => GameplayActionSource.Queue(EntityManager, player, action);
    }
}

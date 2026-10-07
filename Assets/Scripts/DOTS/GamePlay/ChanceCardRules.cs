using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters.CharacterSpawner;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    public static class ChanceCardRules
    {
        // Effects happen once on the server. Replicate the actual cash change,
        // including variable repair and transfer costs, for the card's result.
        public static int Apply(EntityManager manager, EntityCommandBuffer feedback,
            Entity player, ChanceActionDataBuffer card)
        {
            switch (card.effect)
            {
                case ChanceEffect.BuildingRepairs:
                case ChanceEffect.PropertyIncome:
                    int total = 0;
                    using (var query = manager.CreateEntityQuery(typeof(OwnerByEntityComponent)))
                    using (var deeds = query.ToEntityArray(Allocator.Temp))
                        foreach (var deed in deeds)
                        {
                            if (manager.GetComponentData<OwnerByEntityComponent>(deed).Entity != player) continue;
                            if (card.effect == ChanceEffect.PropertyIncome) { total += card.amount; continue; }
                            int buildings = manager.HasComponent<HouseCount>(deed) ? manager.GetComponentData<HouseCount>(deed).Value : 0;
                            total -= buildings >= 5 ? card.hotelAmount : card.amount * buildings;
                        }
                    return ChangeMoney(manager, feedback, player, total);

                case ChanceEffect.PayEachPlayer:
                case ChanceEffect.CollectFromEachPlayer:
                    int transferTotal = 0;
                    using (var query = manager.CreateEntityQuery(typeof(GhostMoneyComponet), typeof(BankruptPlayer)))
                    using (var players = query.ToEntityArray(Allocator.Temp))
                        foreach (var other in players)
                        {
                            var status = manager.GetComponentData<BankruptPlayer>(other);
                            if (other == player || status.Value || status.Declared) continue;
                            int delta = card.effect == ChanceEffect.PayEachPlayer ? -card.amount : card.amount;
                            ChangeMoney(manager, feedback, other, -delta);
                            transferTotal += delta;
                        }
                    // Negative balances enter the existing recovery flow, including
                    // opponents charged outside their turn. No one is eliminated here.
                    return ChangeMoney(manager, feedback, player, transferTotal);

                case ChanceEffect.AdvanceToSpace:
                    Advance(manager, player, card.targetBoardIndex);
                    return 0;

                case ChanceEffect.AdvanceToTransport:
                    int nearest = -1, distance = 40;
                    using (var query = manager.CreateEntityQuery(typeof(BoardIndexComponent), typeof(PropertyRentKindComponent)))
                    using (var deeds = query.ToEntityArray(Allocator.Temp))
                        foreach (var deed in deeds)
                        {
                            if (manager.GetComponentData<PropertyRentKindComponent>(deed).Value != PropertyRentKind.Transport) continue;
                            int index = manager.GetComponentData<BoardIndexComponent>(deed).Value;
                            int moves = ForwardDistance(manager.GetComponentData<PlayerBoardIndex>(player).Value, index);
                            if (moves > 0 && moves < distance) { distance = moves; nearest = index; }
                        }
                    if (nearest >= 0) Advance(manager, player, nearest);
                    return 0;

                case ChanceEffect.GoToJail:
                    SendToJail(manager, player);
                    return 0;

                case ChanceEffect.JailFreeCard:
                    var jail = manager.GetComponentData<JailState>(player);
                    jail.HasGetOutOfJailFreeCard = true;
                    manager.SetComponentData(player, jail);
                    return 0;

                default:
                    return ChangeMoney(manager, feedback, player, card.amount);
            }
        }

        static int ChangeMoney(EntityManager manager, EntityCommandBuffer feedback, Entity player, int amount)
        {
            var money = manager.GetComponentData<GhostMoneyComponet>(player);
            money.Value += amount;
            manager.SetComponentData(player, money);
            MoneyFeedback.Send(feedback, manager, player, amount, MoneyChangeReason.Card);
            return amount;
        }

        public static int ForwardDistance(int from, int to) => (to - from + 40) % 40;

        static void Advance(EntityManager manager, Entity player, int destination)
        {
            if (destination < 0 || destination >= 40) return;
            int moves = ForwardDistance(manager.GetComponentData<PlayerBoardIndex>(player).Value, destination);
            if (moves == 0) return;
            manager.SetComponentData(player, new RemainingMoves { Value = moves });
            manager.SetComponentData(player, new FinalArrived());
            manager.SetComponentData(player, new ReachedTargetPosition());
            manager.SetComponentData(player, new LandingPaymentResolved());
            // Normal walking pays GO once and resolves purchase/rent/tax at the destination.
            using var query = manager.CreateEntityQuery(typeof(GameStateComponent));
            var game = query.GetSingleton<GameStateComponent>();
            game.State = GameState.Walking;
            query.SetSingleton(game);
        }

        public static bool SendToJail(EntityManager manager, Entity player)
        {
            using var jailQuery = manager.CreateEntityQuery(typeof(JailSpaceTag), typeof(BoardIndexComponent));
            using var jails = jailQuery.ToEntityArray(Allocator.Temp);
            using var routeQuery = manager.CreateEntityQuery(typeof(WaypointsBlobRef));
            if (jails.Length == 0 || routeQuery.IsEmptyIgnoreFilter) return false;
            var route = routeQuery.GetSingleton<WaypointsBlobRef>().Reference;
            ref var waypoints = ref route.Value.Waypoints;
            var destination = jails[0];
            int destinationIndex = manager.GetComponentData<BoardIndexComponent>(destination).Value;
            int waypoint = manager.GetComponentData<PlayerWaypointIndex>(player).Value;
            int board = manager.GetComponentData<PlayerBoardIndex>(player).Value;
            for (int step = 0; step < waypoints.Length; step++)
            {
                waypoint = (waypoint + 1) % waypoints.Length;
                if (!waypoints[waypoint].IsLandingSpot) continue;
                board = (board + 1) % 40;
                if (board != destinationIndex) continue;
                var position = waypoints[waypoint].Position;
                var transform = manager.GetComponentData<LocalTransform>(player);
                transform.Position = position;
                manager.SetComponentData(player, transform);
                manager.SetComponentData(player, new PlayerBoardIndex { Value = board });
                manager.SetComponentData(player, new PlayerWaypointIndex { Value = waypoint });
                manager.SetComponentData(player, new SpaceLandedOn { entity = destination });
                manager.SetComponentData(player, new TargetPosition { Value = position });
                manager.SetComponentData(player, new RemainingMoves());
                manager.SetComponentData(player, new PlayerMovementState { Value = MoveState.Idle });
                manager.SetComponentData(player, new FinalArrived());
                manager.SetComponentData(player, new ReachedTargetPosition());
                var jail = manager.GetComponentData<JailState>(player);
                jail.InJail = true;
                jail.TurnsInJail = 2;
                jail.DoublesRollAttempts = 0;
                manager.SetComponentData(player, jail);
                return true;
            }
            return false;
        }

        public static bool UseJailFreeCard(EntityManager manager, Entity player)
        {
            if (!manager.HasComponent<JailState>(player)) return false;
            var jail = manager.GetComponentData<JailState>(player);
            if (!jail.InJail || !jail.HasGetOutOfJailFreeCard) return false;
            jail.HasGetOutOfJailFreeCard = false;
            jail.InJail = false;
            jail.TurnsInJail = jail.DoublesRollAttempts = 0;
            manager.SetComponentData(player, jail);
            return true;
        }
    }
}

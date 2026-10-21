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
            // Chance draws now award cards only. Keep this guard at the rules layer
            // so legacy or newly authored cash/movement cards cannot change state.
            if (card.effect != ChanceEffect.JailFreeCard) return 0;

            switch (card.effect)
            {
                case ChanceEffect.JailFreeCard:
                    CardInventoryRules.Receive(manager, player, PlayerCardType.GetOutOfJailFree);
                    return 0;

                default:
                    return 0;
            }
        }

        public static int ForwardDistance(int from, int to) => (to - from + 40) % 40;

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
            if (manager.HasComponent<PlayerCardInventory>(player))
            {
                var inventory = manager.GetComponentData<PlayerCardInventory>(player);
                for (byte slot = 0; slot < inventory.Count; slot++)
                    if (CardInventoryRules.GetCard(inventory, slot) == PlayerCardType.GetOutOfJailFree)
                        return CardInventoryRules.Use(manager, player, slot);
                return false;
            }
            jail.HasGetOutOfJailFreeCard = false;
            jail.InJail = false;
            jail.TurnsInJail = jail.DoublesRollAttempts = 0;
            manager.SetComponentData(player, jail);
            return true;
        }
    }
}

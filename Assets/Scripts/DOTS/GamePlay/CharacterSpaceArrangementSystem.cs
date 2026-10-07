using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using DOTS.Characters.CharacterSpawner;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    public static class CharacterSpaceLayout
    {
        public const float PieceSpacing = 4f;

        public static float3 Offset(int slot, int count)
        {
            if (count <= 1) return float3.zero;
            float radius = PieceSpacing / (2f * math.sin(math.PI / count));
            float angle = slot * (2f * math.PI / count);
            return new float3(math.cos(angle) * radius, 0f, math.sin(angle) * radius);
        }
    }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(MoveCharacterSystem))]
    [UpdateAfter(typeof(GamePlaySystem))]
    [UpdateAfter(typeof(RollSystem))]
    [UpdateAfter(typeof(JailSystem))]
    [UpdateAfter(typeof(PickRandomChanceCardSystem))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial struct CharacterSpaceArrangementSystem : ISystem
    {
        struct Occupant
        {
            public Entity Entity;
            public int BoardIndex, NetworkId;
            public float3 Center;
        }

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WaypointsBlobRef>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<CharacterFlag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var route = SystemAPI.GetSingleton<WaypointsBlobRef>().Reference;
            if (!route.IsCreated) return;
            ref var waypoints = ref route.Value.Waypoints;
            var game = SystemAPI.GetSingleton<GameStateComponent>();
            Entity departing = Entity.Null;
            if (game.State == GameState.Walking && SystemAPI.TryGetSingleton<CurrentActivePlayer>(out var active))
                departing = active.Entity;

            using var occupants = new NativeList<Occupant>(6, Allocator.Temp);
            foreach (var (board, waypoint, movement, moves, owner, entity) in
                SystemAPI.Query<RefRO<PlayerBoardIndex>, RefRO<PlayerWaypointIndex>, RefRO<PlayerMovementState>,
                    RefRO<RemainingMoves>, RefRO<GhostOwner>>()
                    .WithAll<CharacterFlag, LocalTransform>().WithEntityAccess())
            {
                if (movement.ValueRO.Value != MoveState.Idle || moves.ValueRO.Value > 0 || entity == departing) continue;
                int index = waypoint.ValueRO.Value;
                if (index < 0 || index >= waypoints.Length || !waypoints[index].IsLandingSpot) continue;
                occupants.Add(new Occupant
                {
                    Entity = entity, BoardIndex = board.ValueRO.Value,
                    NetworkId = owner.ValueRO.NetworkId, Center = waypoints[index].Position
                });
            }

            float blend = 1f - math.exp(-12f * math.max(0f, SystemAPI.Time.DeltaTime));
            for (int i = 0; i < occupants.Length; i++)
            {
                var occupant = occupants[i];
                int slot = 0, count = 0;
                for (int j = 0; j < occupants.Length; j++)
                {
                    var other = occupants[j];
                    if (other.BoardIndex != occupant.BoardIndex) continue;
                    count++;
                    // Stable slots regardless of entity query order or which player is active.
                    if (other.NetworkId < occupant.NetworkId ||
                        (other.NetworkId == occupant.NetworkId && other.Entity.Index < occupant.Entity.Index)) slot++;
                }
                float3 destination = occupant.Center + CharacterSpaceLayout.Offset(slot, count);
                var transform = SystemAPI.GetComponentRW<LocalTransform>(occupant.Entity);
                float3 position = math.lerp(transform.ValueRO.Position, destination, blend);
                if (math.distancesq(position, destination) < 0.0001f) position = destination;
                if (math.distancesq(transform.ValueRO.Position, position) > 0f)
                    transform.ValueRW.Position = position;
            }
        }
    }
}

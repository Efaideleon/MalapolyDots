using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.GamePlay;
using Assets.Scripts.DOTS.Mediator;
using DOTS.Characters.CharacterSpawner;
using DOTS.DataComponents;
using DOTS.GameSpaces;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateBefore(typeof(ChangeTurnSystem))]
    public partial struct JailSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkStreamInGame>();
            state.RequireForUpdate<CurrentActivePlayer>();
            state.RequireForUpdate<GameStateComponent>();
            state.RequireForUpdate<WaypointsBlobRef>();
            state.RequireForUpdate<JailSpaceTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (request, _, rpcEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<GoToJailRpc>>().WithEntityAccess())
            {
                ecb.DestroyEntity(rpcEntity);
                var player = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
                var connection = request.ValueRO.SourceConnection;
                if (!SystemAPI.HasComponent<GhostOwner>(player) ||
                    !SystemAPI.HasComponent<NetworkId>(connection) ||
                    SystemAPI.GetComponent<GhostOwner>(player).NetworkId != SystemAPI.GetComponent<NetworkId>(connection).Value ||
                    SystemAPI.GetSingleton<GameStateComponent>().State != GameState.Landing ||
                    !SystemAPI.HasComponent<GoToJailTag>(SystemAPI.GetComponent<SpaceLandedOn>(player).entity))
                {
                    continue;
                }

                foreach (var (jailIndex, jailEntity) in SystemAPI.Query<RefRO<BoardIndexComponent>>().WithAll<JailSpaceTag>().WithEntityAccess())
                {
                    // Follow the same landing-spot counting as normal movement. Corner
                    // waypoints are not board spaces, so board and waypoint indices differ.
                    var route = SystemAPI.GetSingleton<WaypointsBlobRef>().Reference;
                    ref var waypoints = ref route.Value.Waypoints;
                    int waypointIndex = SystemAPI.GetComponent<PlayerWaypointIndex>(player).Value;
                    int boardIndex = SystemAPI.GetComponent<PlayerBoardIndex>(player).Value;
                    bool found = false;
                    for (int step = 0; step < waypoints.Length; step++)
                    {
                        waypointIndex = (waypointIndex + 1) % waypoints.Length;
                        if (!waypoints[waypointIndex].IsLandingSpot) continue;
                        boardIndex = (boardIndex + 1) % 40;
                        if (boardIndex == jailIndex.ValueRO.Value)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) break;

                    var position = waypoints[waypointIndex].Position;
                    SystemAPI.GetComponentRW<LocalTransform>(player).ValueRW.Position = position;
                    SystemAPI.SetComponent(player, new PlayerBoardIndex { Value = boardIndex });
                    SystemAPI.SetComponent(player, new PlayerWaypointIndex { Value = waypointIndex });
                    SystemAPI.SetComponent(player, new SpaceLandedOn { entity = jailEntity });
                    SystemAPI.SetComponent(player, new TargetPosition { Value = position });
                    SystemAPI.SetComponent(player, new RemainingMoves { Value = 0 });
                    SystemAPI.SetComponent(player, new PlayerMovementState { Value = MoveState.Idle });
                    SystemAPI.SetComponent(player, new FinalArrived { Value = false });
                    SystemAPI.SetComponent(player, new ReachedTargetPosition { Value = false });
                    var jailState = SystemAPI.GetComponentRW<JailState>(player);
                    jailState.ValueRW.InJail = true;
                    jailState.ValueRW.TurnsInJail = 2;
                    jailState.ValueRW.DoublesRollAttempts = 0;

                    // Queue the existing turn-change handler only after the transfer.
                    var turnRequest = ecb.CreateEntity();
                    ecb.AddComponent<ChangeTurnRpc>(turnRequest);
                    ecb.AddComponent(turnRequest, request.ValueRO);
                    break;
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

using Assets.Scripts.DOTS.Characters;
using Assets.Scripts.DOTS.Mediator;
using Assets.Scripts.DOTS.GamePlay;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.GamePlay.ChanceActionSystems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct ChanceTransactionBufferRouterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<ActivePlayer>();
            state.EntityManager.CreateSingletonBuffer<ChanceBufferEvent>();
            state.RequireForUpdate<ChanceBufferEvent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (request, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<ChanceRpc>().WithEntityAccess())
            {
                var connection = request.ValueRO.SourceConnection;
                var active = SystemAPI.GetSingleton<CurrentActivePlayer>().Entity;
                if (active != Entity.Null && GameplayActionSource.Owns(state.EntityManager, entity, connection, active))
                {
                    var game = SystemAPI.GetSingletonRW<GameStateComponent>();
                    if (game.ValueRO.State == GameState.CardAwaitingAcceptance)
                        game.ValueRW.State = GameState.Walking;
                    SystemAPI.GetSingletonBuffer<ChanceBufferEvent>().Add(new ChanceBufferEvent { });
                }
                ecb.DestroyEntity(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    public struct ChanceBufferEvent : IBufferElementData
    { }
}
